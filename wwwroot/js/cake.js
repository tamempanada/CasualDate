const cakeSteps = Array.from(document.querySelectorAll("[data-cake-step]"));
const stepDots = Array.from(document.querySelectorAll("[data-step-dot]"));
const previousButton = document.querySelector("#previousCakeStep");
const nextButton = document.querySelector("#nextCakeStep");
const resultPanel = document.querySelector("#cakeResult");
const resultTitle = document.querySelector("#cakeResultTitle");
const resultMessage = document.querySelector("#cakeResultMessage");
const resultImage = document.querySelector("#cakeResultImage");
const cakeAnimatedScore = document.querySelector("#cakeAnimatedScore");
const cakeSummary = document.querySelector("#cakeSummary");
const cakeBase = document.querySelector("#cakeBase");
const cakeFrosting = document.querySelector("#cakeFrosting");
const cakeTopping = document.querySelector("#cakeTopping");
const cakeDecoration = document.querySelector("#cakeDecoration");
const cakeResultBase = document.querySelector("#cakeResultBase");
const cakeResultFrosting = document.querySelector("#cakeResultFrosting");
const cakeResultTopping = document.querySelector("#cakeResultTopping");
const cakeResultDecoration = document.querySelector("#cakeResultDecoration");
const cakePage = document.querySelector(".cake-page");
const cakeResultRanges = JSON.parse(cakePage?.dataset.resultRanges || "[]");

const choices = {};
let currentStep = 0;

function showStep(index) {
  currentStep = Math.max(0, Math.min(index, cakeSteps.length - 1));

  cakeSteps.forEach((step, stepIndex) => {
    step.classList.toggle("active", stepIndex === currentStep);
  });

  stepDots.forEach((dot, stepIndex) => {
    dot.classList.toggle("active", stepIndex === currentStep);
  });

  previousButton.disabled = currentStep === 0;
  nextButton.textContent = currentStep === cakeSteps.length - 1 ? "Finish Cake" : "Next";
}

function selectOption(button) {
  const step = button.closest("[data-cake-step]");
  const type = step.dataset.choiceType;

  choices[type] = {
    id: Number(button.dataset.choiceId),
    name: button.dataset.choiceName,
    score: Number(button.dataset.choiceScore),
    color: button.dataset.choiceColor,
    image: button.dataset.choiceImage || ""
  };

  step.querySelectorAll(".cake-option").forEach((option) => {
    option.classList.toggle("selected", option === button);
  });

  updateCakePreview();
}

function updateCakePreview() {
  if (choices.base) {
    applyCakeLayer(cakeBase, choices.base, choices.base.color);
  }

  if (choices.frosting) {
    applyCakeLayer(cakeFrosting, choices.frosting, choices.frosting.color);
  }

  if (choices.topping) {
    applyCakeLayer(cakeTopping, choices.topping, toppingBackground(choices.topping));
  }

  if (choices.decoration) {
    cakeDecoration.style.color = choices.decoration.color;
    applyCakeLayer(cakeDecoration, choices.decoration, "transparent");
    cakeDecoration.textContent = choices.decoration.image
      ? ""
      : decorationSymbol(choices.decoration.name);
  }
}

function applyCakeLayer(element, choice, fallbackBackground) {
  element.classList.toggle("has-image", Boolean(choice.image));
  element.style.background = fallbackBackground;

  if (choice.image) {
    element.style.backgroundImage = `url("${choice.image}")`;
  }
}

function decorationSymbol(name) {
  const normalized = name.toLowerCase();

  if (normalized.includes("heart")) {
    return "♥ ♥ ♥";
  }

  if (normalized.includes("flower")) {
    return "✿ ✿ ✿";
  }

  if (normalized.includes("candle")) {
    return "|||";
  }

  if (normalized.includes("initial")) {
    return "A + M";
  }

  return "★ ★ ★";
}

async function finishCake() {
  const required = ["base", "frosting", "topping", "decoration"];
  const missing = required.find((key) => !choices[key]);

  if (missing) {
    showStep(required.indexOf(missing));
    return;
  }

  nextButton.disabled = true;
  let completion;
  try {
    completion = await window.CasualDateGameRewards.complete("cake", {
      baseId: choices.base.id,
      frostingId: choices.frosting.id,
      toppingId: choices.topping.id,
      decorationId: choices.decoration.id
    });
  } catch (error) {
    nextButton.textContent = error.message;
    nextButton.disabled = false;
    return;
  }

  if (!completion?.result) {
    nextButton.textContent = "Try again";
    nextButton.disabled = false;
    return;
  }

  const score = completion.score;
  const result = completion.result;

  resultTitle.textContent = result.Title || result.title;
  resultMessage.textContent = result.Message || result.message;
  animateNumber(cakeAnimatedScore, score);
  showResultImage(resultImage, result.imagePath);
  updateCakeResultPreview();
  cakeSummary.innerHTML = "";

  required.forEach((key) => {
    const item = document.createElement("li");
    item.textContent = `${labelFor(key)}: ${choices[key].name}`;
    cakeSummary.appendChild(item);
  });

  resultPanel.hidden = false;
}

function getCakeResult(score) {
  const matchingRange = cakeResultRanges.find((range) => score >= range.MinScore && score <= range.MaxScore);

  if (matchingRange) {
    return matchingRange;
  }

  const allNames = Object.values(choices).map((choice) => choice.name.toLowerCase()).join(" ");

  if ((allNames.match(/chocolate/g) || []).length >= 3) {
    return {
      title: "Chocolate Emergency",
      message: "You REALLY like chocolate. Honestly, respectable."
    };
  }

  if (score >= 92) {
    return {
      title: "Perfect Cake",
      message: "This is dangerously close to proposal-level baking."
    };
  }

  if (score >= 78) {
    return {
      title: "Very Cute Cake",
      message: "Sweet, thoughtful, and absolutely date-approved."
    };
  }

  if (score >= 60) {
    return {
      title: "Cozy Cake",
      message: "A little chaotic, but in a charming way."
    };
  }

  return {
    title: "Experimental Cake",
    message: "We may need to taste-test this together for science."
  };
}

function updateCakeResultPreview() {
  if (choices.base) {
    applyCakeLayer(cakeResultBase, choices.base, choices.base.color);
  }

  if (choices.frosting) {
    applyCakeLayer(cakeResultFrosting, choices.frosting, choices.frosting.color);
  }

  if (choices.topping) {
    applyCakeLayer(cakeResultTopping, choices.topping, toppingBackground(choices.topping));
  }

  if (choices.decoration) {
    cakeResultDecoration.style.color = choices.decoration.color;
    applyCakeLayer(cakeResultDecoration, choices.decoration, "transparent");
    cakeResultDecoration.textContent = choices.decoration.image
      ? ""
      : decorationSymbol(choices.decoration.name);
  }
}

function showResultImage(image, imagePath) {
  if (!image) {
    return;
  }

  if (!imagePath) {
    image.hidden = true;
    image.removeAttribute("src");
    return;
  }

  image.onerror = () => {
    image.hidden = true;
    image.removeAttribute("src");
  };
  image.src = imagePath;
  image.hidden = false;
}

function toppingBackground(choice) {
  const color = choice.color || "#d84f6f";
  const normalized = choice.name.toLowerCase();

  if (normalized.includes("sprinkle")) {
    return `radial-gradient(circle at 12% 45%, ${color} 0 4px, transparent 5px),
      radial-gradient(circle at 28% 28%, #f6c95f 0 3px, transparent 4px),
      radial-gradient(circle at 43% 58%, #7bb8a5 0 4px, transparent 5px),
      radial-gradient(circle at 62% 32%, ${color} 0 3px, transparent 4px),
      radial-gradient(circle at 78% 56%, #f6c95f 0 4px, transparent 5px),
      linear-gradient(180deg, rgba(255,255,255,0.36), rgba(255,255,255,0.05))`;
  }

  if (normalized.includes("strawberry") || normalized.includes("berry")) {
    return `radial-gradient(ellipse at 18% 52%, ${color} 0 10px, transparent 11px),
      radial-gradient(ellipse at 39% 34%, ${color} 0 9px, transparent 10px),
      radial-gradient(ellipse at 60% 58%, ${color} 0 10px, transparent 11px),
      radial-gradient(ellipse at 82% 39%, ${color} 0 9px, transparent 10px),
      linear-gradient(180deg, rgba(255,255,255,0.42), rgba(255,255,255,0.08))`;
  }

  if (normalized.includes("cherr")) {
    return `radial-gradient(circle at 18% 54%, #c9263f 0 9px, transparent 10px),
      radial-gradient(circle at 36% 36%, #d9364e 0 8px, transparent 9px),
      radial-gradient(circle at 56% 57%, #c9263f 0 9px, transparent 10px),
      radial-gradient(circle at 77% 38%, #d9364e 0 8px, transparent 9px),
      linear-gradient(180deg, rgba(255,255,255,0.42), rgba(255,255,255,0.08))`;
  }

  if (normalized.includes("cookie")) {
    return `radial-gradient(circle at 18% 52%, #8c5a32 0 10px, transparent 11px),
      radial-gradient(circle at 39% 34%, #b87945 0 9px, transparent 10px),
      radial-gradient(circle at 61% 58%, #8c5a32 0 10px, transparent 11px),
      radial-gradient(circle at 82% 39%, #b87945 0 9px, transparent 10px),
      linear-gradient(180deg, rgba(255,255,255,0.36), rgba(255,255,255,0.08))`;
  }

  return `repeating-linear-gradient(135deg, ${color} 0 10px, rgba(255,255,255,0.42) 10px 18px),
    linear-gradient(180deg, rgba(255,255,255,0.28), rgba(255,255,255,0.06))`;
}

function animateNumber(element, finalValue) {
  if (!element) {
    return;
  }

  const duration = 720;
  const startTime = performance.now();

  function tick(now) {
    const progress = Math.min((now - startTime) / duration, 1);
    const eased = 1 - Math.pow(1 - progress, 3);
    element.textContent = String(Math.round(finalValue * eased));

    if (progress < 1) {
      window.requestAnimationFrame(tick);
      return;
    }

    element.textContent = String(finalValue);
  }

  window.requestAnimationFrame(tick);
}

function labelFor(key) {
  return key.charAt(0).toUpperCase() + key.slice(1);
}

document.querySelectorAll(".cake-option").forEach((button) => {
  button.addEventListener("click", () => selectOption(button));
});

previousButton?.addEventListener("click", () => showStep(currentStep - 1));
nextButton?.addEventListener("click", () => {
  if (currentStep === cakeSteps.length - 1) {
    void finishCake();
    return;
  }

  showStep(currentStep + 1);
});

showStep(0);
