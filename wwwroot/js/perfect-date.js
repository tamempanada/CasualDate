const datePage = document.querySelector(".perfect-date-page");
const dateSteps = Array.from(document.querySelectorAll("[data-date-step]"));
const dateStepperItems = Array.from(document.querySelectorAll(".date-stepper span"));
const previousDateStepButton = document.querySelector("#previousDateStepButton");
const nextDateStepButton = document.querySelector("#nextDateStepButton");
const datePlanTitle = document.querySelector("#datePlanTitle");
const datePlanSummary = document.querySelector("#datePlanSummary");
const dateScore = document.querySelector("#dateScore");
const dateResult = document.querySelector("#dateResult");
const dateResultTitle = document.querySelector("#dateResultTitle");
const dateResultMessage = document.querySelector("#dateResultMessage");
const dateResultImage = document.querySelector("#dateResultImage");
const dateAnimatedScore = document.querySelector("#dateAnimatedScore");

let currentDateStep = 0;
const dateChoices = {};
const resultRanges = JSON.parse(datePage?.dataset.resultRanges || "[]");

function showDateStep(index) {
  currentDateStep = Math.max(0, Math.min(index, dateSteps.length - 1));

  dateSteps.forEach((step, stepIndex) => {
    step.classList.toggle("active", stepIndex === currentDateStep);
  });

  dateStepperItems.forEach((item, stepIndex) => {
    item.classList.toggle("active", stepIndex === currentDateStep);
  });

  previousDateStepButton.disabled = currentDateStep === 0;
  nextDateStepButton.textContent = currentDateStep === dateSteps.length - 1 ? "Reveal Date" : "Next";
}

function selectDateOption(button) {
  const step = button.closest("[data-date-step]");
  const stepIndex = Number(step.dataset.dateStep);

  dateChoices[stepIndex] = {
    id: button.dataset.choiceId,
    name: button.dataset.choiceName,
    score: Number(button.dataset.choiceScore),
    image: button.dataset.choiceImage || ""
  };

  step.querySelectorAll(".date-option").forEach((option) => {
    option.classList.toggle("selected", option === button);
  });

  updateDateSummary();
}

function getDateScore() {
  return Object.values(dateChoices).reduce((total, choice) => total + choice.score, 0);
}

function updateDateSummary() {
  const choices = Object.values(dateChoices);

  dateScore.textContent = "Hidden";
  datePlanTitle.textContent = choices.length === 0
    ? "Choose your date"
    : `${choices.length} / ${dateSteps.length} choices`;

  datePlanSummary.innerHTML = "";

  if (choices.length === 0) {
    const item = document.createElement("li");
    item.textContent = "No choices yet.";
    datePlanSummary.appendChild(item);
    return;
  }

  choices.forEach((choice) => {
    const item = document.createElement("li");

    if (choice.image) {
      const image = document.createElement("img");
      image.src = choice.image;
      image.alt = "";
      image.loading = "lazy";
      image.addEventListener("error", () => image.remove(), { once: true });
      item.appendChild(image);
    }

    const label = document.createElement("span");
    label.textContent = choice.name;
    item.appendChild(label);

    datePlanSummary.appendChild(item);
  });
}

function getDateResult(score) {
  const matchingRanges = resultRanges.filter((range) => score >= range.MinScore && score <= range.MaxScore);

  if (matchingRanges.length > 0) {
    return matchingRanges[Math.floor(Math.random() * matchingRanges.length)];
  }

  if (score >= 92) {
    return {
      title: "Perfect Date Material",
      message: "This plan has second-date energy written all over it."
    };
  }

  if (score >= 80) {
    return {
      title: "Very Good Date",
      message: "This is sweet, charming, and absolutely worth doing."
    };
  }

  return {
    title: "Cute and Casual",
    message: "Low pressure, cozy, and still very date-worthy."
  };
}

function revealDateResult() {
  if (Object.keys(dateChoices).length < dateSteps.length) {
    datePlanTitle.textContent = "Pick one from each step";
    return;
  }

  const score = getDateScore();
  const result = getDateResult(score);

  dateScore.textContent = String(score);
  dateResultTitle.textContent = result.Title || result.title;
  dateResultMessage.textContent = result.Message || result.message;
  animateNumber(dateAnimatedScore, score);
  showResultImage(dateResultImage, result.ImagePath || result.imagePath);
  dateResult.hidden = false;
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

document.querySelectorAll(".date-option").forEach((button) => {
  button.addEventListener("click", () => selectDateOption(button));
});

previousDateStepButton?.addEventListener("click", () => {
  showDateStep(currentDateStep - 1);
});

nextDateStepButton?.addEventListener("click", () => {
  if (currentDateStep === dateSteps.length - 1) {
    revealDateResult();
    return;
  }

  showDateStep(currentDateStep + 1);
});

showDateStep(0);
updateDateSummary();
