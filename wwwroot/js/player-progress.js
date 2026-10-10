const progressKey = "casual-date-progress";
const gameIds = Array.from(document.querySelectorAll("[data-game-card]"))
  .map((card) => card.getAttribute("data-game-card"))
  .filter(Boolean);
const finalScreen = document.querySelector(".final-reward-screen");

function getProgress() {
  const saved = window.localStorage.getItem(progressKey);
  return saved ? JSON.parse(saved) : {};
}

function saveProgress(progress) {
  window.localStorage.setItem(progressKey, JSON.stringify(progress));
}

function updateMenuProgress() {
  const progress = getProgress();
  const completedCount = gameIds.filter((id) => progress[id]?.completed).length;
  const summary = document.querySelector("#progressSummary");
  const progressBar = document.querySelector("#progressBar");
  const progressHint = document.querySelector("#progressHint");
  const finalRewardLink = document.querySelector("#finalRewardLink");
  const finalRewardText = document.querySelector("#finalRewardText");
  const finalRewardHint = document.querySelector("#finalRewardHint");
  const nextGameId = gameIds.find((id) => !progress[id]?.completed);

  if (summary) {
    summary.textContent = `${completedCount} / ${gameIds.length} completed`;
  }

  if (progressBar) {
    const percent = gameIds.length === 0 ? 0 : Math.round((completedCount / gameIds.length) * 100);
    progressBar.style.width = `${percent}%`;
  }

  if (progressHint) {
    if (gameIds.length === 0) {
      progressHint.textContent = "No enabled mini-games are available yet.";
    } else if (!nextGameId) {
      progressHint.textContent = "Everything is complete. Your final reward is ready.";
    } else {
      const nextCard = document.querySelector(`[data-game-card="${nextGameId}"]`);
      const nextName = nextCard?.querySelector(".game-copy strong")?.textContent?.trim() || "the next mini-game";
      progressHint.textContent = `Next up: ${nextName}.`;
    }
  }

  document.querySelectorAll("[data-game-card]").forEach((card) => {
    const gameId = card.getAttribute("data-game-card");
    const isCompleted = Boolean(progress[gameId]?.completed);
    const label = card.querySelector(".completion-label");
    const nextLabel = card.querySelector(".next-game-label");
    const actionLabel = card.querySelector(".game-action-label");
    const isNext = !isCompleted && gameId === nextGameId;
    const progressDot = document.querySelector(`[data-progress-dot="${gameId}"]`);

    card.classList.toggle("completed", isCompleted);
    card.classList.toggle("next-game", isNext);

    if (label) {
      label.textContent = isCompleted ? "Completed" : "Not completed";
    }

    if (actionLabel) {
      actionLabel.textContent = isCompleted ? "Play again" : isNext ? "Start here" : "Play";
    }

    if (nextLabel) {
      nextLabel.hidden = !isNext;
    }

    if (progressDot) {
      progressDot.classList.toggle("completed", isCompleted);
      progressDot.classList.toggle("next", isNext);
    }
  });

  if (finalRewardLink && finalRewardText) {
    const isUnlocked = gameIds.length > 0 && completedCount === gameIds.length;

    finalRewardLink.classList.toggle("locked", !isUnlocked);
    finalRewardLink.setAttribute("aria-disabled", String(!isUnlocked));
    finalRewardText.textContent = isUnlocked
      ? "Unlocked. Open your final message."
      : "Complete every game to unlock this.";

    if (finalRewardHint) {
      finalRewardHint.textContent = isUnlocked
        ? "The last surprise is ready."
        : `${Math.max(gameIds.length - completedCount, 0)} mini-game${gameIds.length - completedCount === 1 ? "" : "s"} left.`;
    }
  }
}

function getFinalRequiredGames() {
  if (!finalScreen) {
    return [];
  }

  return JSON.parse(finalScreen.dataset.requiredGames || "[]");
}

function updateFinalReward() {
  if (!finalScreen) {
    return;
  }

  const requiredGames = getFinalRequiredGames();
  const progress = getProgress();
  const completedCount = requiredGames.filter((id) => progress[id]?.completed).length;
  const isUnlocked = requiredGames.length > 0 && completedCount === requiredGames.length;
  const lockedPanel = document.querySelector("#finalLockedPanel");
  const unlockedPanel = document.querySelector("#finalUnlockedPanel");
  const lockedMessage = document.querySelector("#finalLockedMessage");

  if (lockedPanel) {
    lockedPanel.hidden = isUnlocked;
  }

  if (unlockedPanel) {
    unlockedPanel.hidden = !isUnlocked;
  }

  if (lockedMessage) {
    lockedMessage.textContent = `Complete every mini-game first. Progress: ${completedCount} / ${requiredGames.length}.`;
  }
}

function showCompletionNotice(button) {
  const notice = document.createElement("p");
  notice.className = "completion-notice";
  notice.textContent = "Saved. Returning to the menu...";
  notice.setAttribute("role", "status");

  button.insertAdjacentElement("afterend", notice);
}

function navigateToMenu() {
  if (window.CasualDateNavigate) {
    window.CasualDateNavigate("/Home/Menu");
    return;
  }

  window.location.href = "/Home/Menu";
}

document.querySelectorAll("[data-complete-game]").forEach((button) => {
  button.addEventListener("click", () => {
    const gameId = button.getAttribute("data-complete-game");
    const progress = getProgress();

    progress[gameId] = {
      completed: true,
      completedAt: new Date().toISOString()
    };

    saveProgress(progress);
    button.textContent = "Completed";
    button.disabled = true;
    showCompletionNotice(button);
    updateMenuProgress();

    window.setTimeout(() => {
      navigateToMenu();
    }, 900);
  });
});

document.querySelector("#finalRewardLink")?.addEventListener("click", (event) => {
  const link = event.currentTarget;

  if (link.getAttribute("aria-disabled") === "true") {
    event.preventDefault();
  }
});

document.querySelector("#resetProgressButton")?.addEventListener("click", () => {
  window.localStorage.removeItem(progressKey);
  updateMenuProgress();
});

document.querySelector("#replayDateButton")?.addEventListener("click", () => {
  window.localStorage.removeItem(progressKey);
  navigateToMenu();
});

updateMenuProgress();
updateFinalReward();
