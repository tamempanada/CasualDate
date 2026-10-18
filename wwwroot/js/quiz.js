const quizPage = document.querySelector(".quiz-page");
const quizQuestions = Array.from(document.querySelectorAll("[data-quiz-question]"));
const previousQuizButton = document.querySelector("#previousQuizButton");
const nextQuizButton = document.querySelector("#nextQuizButton");
const quizProgressTitle = document.querySelector("#quizProgressTitle");
const quizProgressBar = document.querySelector("#quizProgressBar");
const quizScore = document.querySelector("#quizScore");
const quizResult = document.querySelector("#quizResult");
const quizResultTitle = document.querySelector("#quizResultTitle");
const quizResultMessage = document.querySelector("#quizResultMessage");
const quizResultImage = document.querySelector("#quizResultImage");
const quizAnimatedScore = document.querySelector("#quizAnimatedScore");

let currentQuizQuestion = 0;
const quizAnswers = {};
const maxQuizScore = Number(quizPage?.dataset.maxScore || 0);
const quizResultRanges = JSON.parse(quizPage?.dataset.resultRanges || "[]");

function showQuizQuestion(index) {
  currentQuizQuestion = Math.max(0, Math.min(index, quizQuestions.length - 1));

  quizQuestions.forEach((question, questionIndex) => {
    question.classList.toggle("active", questionIndex === currentQuizQuestion);
  });

  previousQuizButton.disabled = currentQuizQuestion === 0;
  nextQuizButton.textContent = currentQuizQuestion === quizQuestions.length - 1 ? "Reveal Score" : "Next";
  updateQuizScore();
}

async function selectQuizAnswer(button) {
  const question = button.closest("[data-quiz-question]");
  const questionIndex = Number(question.dataset.quizQuestion);
  const questionId = Number(question.dataset.questionId);
  const answerId = Number(button.dataset.answerId);

  question.querySelectorAll(".quiz-answer").forEach((answer) => {
    answer.disabled = true;
  });

  let data;
  try {
    const response = await fetch("/quiz/answer", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ questionId, answerId })
    });

    if (response.status === 401) {
      window.location.href = `/player/login?returnUrl=${encodeURIComponent(window.location.pathname)}`;
      return;
    }

    data = await response.json().catch(() => ({}));
    if (!response.ok) {
      throw new Error(data.message || "Quiz answer could not be checked.");
    }
  } catch (error) {
    question.querySelectorAll(".quiz-answer").forEach((answer) => {
      answer.disabled = false;
    });
    quizProgressTitle.textContent = error.message;
    return;
  }

  quizAnswers[questionIndex] = {
    isCorrect: Boolean(data.isCorrect),
    points: Number(data.points || 0)
  };

  question.querySelectorAll(".quiz-answer").forEach((answer) => {
    answer.classList.toggle("selected", answer === button);
  });

  if (data.reward?.awarded && data.reward.amount > 0) {
    quizProgressTitle.textContent = `Correct! +${data.reward.amount} tokens`;
  } else {
    updateQuizScore();
  }
}

function getCurrentQuizScore() {
  return Object.values(quizAnswers).reduce((total, answer) => total + answer.points, 0);
}

function updateQuizScore() {
  const answeredCount = Object.keys(quizAnswers).length;
  const progressPercent = quizQuestions.length === 0
    ? 0
    : Math.round((answeredCount / quizQuestions.length) * 100);

  quizScore.textContent = "Hidden";
  quizProgressTitle.textContent = `Question ${currentQuizQuestion + 1} / ${quizQuestions.length}`;
  quizProgressBar.style.width = `${progressPercent}%`;
}

function getQuizResult(score) {
  const customRange = quizResultRanges.find((range) => score >= range.MinScore && score <= range.MaxScore);

  if (customRange) {
    return {
      title: customRange.Title,
      message: customRange.Message,
      imagePath: customRange.ImagePath
    };
  }

  const percent = maxQuizScore === 0 ? 0 : Math.round((score / maxQuizScore) * 100);

  if (percent >= 90) {
    return {
      title: "Almost suspiciously good",
      message: `You scored ${score} / ${maxQuizScore}. This is elite attention-to-detail energy.`
    };
  }

  if (percent >= 70) {
    return {
      title: "Very date-worthy",
      message: `You scored ${score} / ${maxQuizScore}. A strong showing with plenty of charm.`
    };
  }

  return {
    title: "Cute attempt",
    message: `You scored ${score} / ${maxQuizScore}. Still counts, but this deserves a playful rematch.`
  };
}

function revealQuizResult() {
  if (Object.keys(quizAnswers).length < quizQuestions.length) {
    quizProgressTitle.textContent = "Answer every question first";
    return;
  }

  const score = getCurrentQuizScore();
  const result = getQuizResult(score);

  quizScore.textContent = String(score);
  quizResultTitle.textContent = result.title;
  quizResultMessage.textContent = result.message;
  animateNumber(quizAnimatedScore, score);
  showResultImage(quizResultImage, result.imagePath);
  quizResult.hidden = false;
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

document.querySelectorAll(".quiz-answer").forEach((button) => {
  button.addEventListener("click", () => selectQuizAnswer(button));
});

previousQuizButton?.addEventListener("click", () => {
  showQuizQuestion(currentQuizQuestion - 1);
});

nextQuizButton?.addEventListener("click", () => {
  if (currentQuizQuestion === quizQuestions.length - 1) {
    revealQuizResult();
    return;
  }

  showQuizQuestion(currentQuizQuestion + 1);
});

showQuizQuestion(0);
