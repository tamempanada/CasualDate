const hangmanRoot = document.querySelector("[data-hangman-root]");

if (hangmanRoot) {
  const words = window.CasualDateHangmanWords;
  const allWords = [...words.easy, ...words.medium, ...words.hard];
  const adventureWords = [
    ...words.easy.slice(0, 5),
    ...words.medium.slice(0, 5),
    ...words.hard.slice(0, 5),
  ];
  const alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".split("");
  const levelGrid = hangmanRoot.querySelector("[data-level-grid]");
  const keyboard = hangmanRoot.querySelector("[data-keyboard]");
  const wordDisplay = hangmanRoot.querySelector("[data-word-display]");
  const person = hangmanRoot.querySelector("[data-hangman-person]");
  const message = hangmanRoot.querySelector("[data-message]");
  const prompt = hangmanRoot.querySelector("[data-prompt]");
  const runLabel = hangmanRoot.querySelector("[data-run-label]");
  const missesNode = hangmanRoot.querySelector("[data-misses]");
  const streakNode = hangmanRoot.querySelector("[data-streak]");
  const unlockedNode = hangmanRoot.querySelector("[data-unlocked-level]");
  const endlessWinsNode = hangmanRoot.querySelector("[data-endless-wins]");
  const bestStreakNode = hangmanRoot.querySelector("[data-best-streak]");
  let progress = { adventureUnlockedLevel: 1, adventureCompletedLevels: [], endlessTotalWins: 0, endlessBestStreak: 0 };
  let mode = "adventure";
  let level = 1;
  let currentWord = "";
  let guessed = new Set();
  let misses = 0;
  let ended = true;
  let streak = 0;
  let endlessPool = [];

  const requestJson = async (url, options = {}) => {
    const response = await fetch(url, {
      headers: { "Content-Type": "application/json", ...(options.headers || {}) },
      ...options,
    });

    if (response.status === 401) {
      window.location.href = `/player/login?returnUrl=${encodeURIComponent(window.location.pathname)}`;
      return null;
    }

    const data = await response.json().catch(() => ({}));

    if (!response.ok) {
      throw new Error(data.message || "Hangman is unavailable.");
    }

    return data;
  };

  const shuffle = (items) => {
    const copy = [...items];
    for (let index = copy.length - 1; index > 0; index--) {
      const swapIndex = Math.floor(Math.random() * (index + 1));
      [copy[index], copy[swapIndex]] = [copy[swapIndex], copy[index]];
    }
    return copy;
  };

  const renderProgress = () => {
    unlockedNode.textContent = progress.adventureUnlockedLevel;
    endlessWinsNode.textContent = progress.endlessTotalWins;
    bestStreakNode.textContent = progress.endlessBestStreak;
    levelGrid.innerHTML = adventureWords
      .map((_, index) => {
        const levelNumber = index + 1;
        const locked = levelNumber > progress.adventureUnlockedLevel;
        const done = progress.adventureCompletedLevels.includes(levelNumber);
        return `<button type="button" data-level="${levelNumber}" ${locked ? "disabled" : ""} class="${done ? "is-complete" : ""}">${levelNumber}</button>`;
      })
      .join("");

    levelGrid.querySelectorAll("[data-level]").forEach((button) => {
      button.addEventListener("click", () => startAdventure(Number(button.dataset.level)));
    });
  };

  const renderKeyboard = () => {
    keyboard.innerHTML = alphabet
      .map((letter) => `<button type="button" data-letter="${letter}" ${guessed.has(letter) || ended ? "disabled" : ""}>${letter}</button>`)
      .join("");
    keyboard.querySelectorAll("[data-letter]").forEach((button) => {
      button.addEventListener("click", () => guessLetter(button.dataset.letter));
    });
  };

  const renderWord = () => {
    wordDisplay.innerHTML = currentWord
      .split("")
      .map((letter) => `<span>${guessed.has(letter.toUpperCase()) || ended && misses >= 6 ? letter.toUpperCase() : ""}</span>`)
      .join("");
  };

  const renderState = () => {
    person.dataset.misses = String(misses);
    missesNode.textContent = misses;
    streakNode.textContent = streak;
    renderWord();
    renderKeyboard();
  };

  const startRound = (word, label) => {
    currentWord = word.toUpperCase();
    guessed = new Set();
    misses = 0;
    ended = false;
    prompt.textContent = label;
    message.textContent = "Pick a letter.";
    renderState();
  };

  const startAdventure = (selectedLevel) => {
    mode = "adventure";
    level = selectedLevel;
    runLabel.textContent = `Adventure level ${level}`;
    startRound(adventureWords[level - 1], difficultyForLevel(level));
  };

  const startEndless = () => {
    mode = "endless";
    runLabel.textContent = "Endless";
    if (endlessPool.length === 0) {
      endlessPool = shuffle(allWords);
    }
    const word = endlessPool.pop();
    startRound(word, "Endless word");
  };

  const difficultyForLevel = (levelNumber) => {
    if (levelNumber <= 5) return "Easy word";
    if (levelNumber <= 10) return "Medium word";
    return "Hard word";
  };

  const saveAdventureWin = async () => {
    const data = await requestJson("/hangman/adventure-complete", {
      method: "POST",
      body: JSON.stringify({ level }),
    });
    if (data?.progress) {
      progress = data.progress;
      renderProgress();
    }
    return data?.reward;
  };

  const saveEndlessWin = async () => {
    const data = await requestJson("/hangman/endless-win", {
      method: "POST",
      body: JSON.stringify({ streak }),
    });
    if (data?.progress) {
      progress = data.progress;
      renderProgress();
    }
    return data?.reward;
  };

  const finishRound = async (won) => {
    ended = true;
    if (won) {
      streak += 1;
      let reward;
      if (mode === "adventure") {
        reward = await saveAdventureWin();
      } else {
        reward = await saveEndlessWin();
      }
      const rewardText = reward?.awarded && reward.amount > 0
        ? ` +${reward.amount} tokens.`
        : "";
      message.textContent = mode === "adventure"
        ? `You cleared the level.${rewardText}`
        : `Correct. Keep the streak going.${rewardText}`;
    } else {
      message.textContent = `The word was ${currentWord}.`;
      streak = 0;
    }
    renderState();
  };

  const guessLetter = (letter) => {
    if (ended || guessed.has(letter)) return;
    guessed.add(letter);
    if (!currentWord.includes(letter)) {
      misses += 1;
    }
    const won = currentWord.split("").every((item) => guessed.has(item));
    if (won || misses >= 6) {
      finishRound(won);
      return;
    }
    renderState();
  };

  document.addEventListener("keydown", (event) => {
    const letter = event.key.toUpperCase();
    if (/^[A-Z]$/.test(letter)) {
      guessLetter(letter);
    }
  });

  hangmanRoot.querySelector("[data-mode='adventure']").addEventListener("click", () => startAdventure(progress.adventureUnlockedLevel));
  hangmanRoot.querySelector("[data-mode='endless']").addEventListener("click", startEndless);
  hangmanRoot.querySelector("[data-replay]").addEventListener("click", () => {
    if (mode === "adventure") startAdventure(level);
    else startEndless();
  });
  hangmanRoot.querySelector("[data-next-word]").addEventListener("click", () => {
    if (mode === "adventure") startAdventure(Math.min(progress.adventureUnlockedLevel, level + 1));
    else startEndless();
  });

  requestJson("/hangman/progress")
    .then((data) => {
      if (data?.progress) {
        progress = data.progress;
      }
      renderProgress();
      startAdventure(progress.adventureUnlockedLevel);
    })
    .catch((error) => {
      message.textContent = error.message;
      renderProgress();
      startAdventure(1);
    });
}
