const blackjackRoot = document.querySelector("[data-blackjack-root]");

if (blackjackRoot) {
  const balanceNode = blackjackRoot.querySelector("[data-balance]");
  const betInput = blackjackRoot.querySelector("[data-bet-input]");
  const dealButton = blackjackRoot.querySelector("[data-deal]");
  const hitButton = blackjackRoot.querySelector("[data-hit]");
  const standButton = blackjackRoot.querySelector("[data-stand]");
  const dealerCardsNode = blackjackRoot.querySelector("[data-dealer-cards]");
  const playerCardsNode = blackjackRoot.querySelector("[data-player-cards]");
  const dealerValueNode = blackjackRoot.querySelector("[data-dealer-value]");
  const playerValueNode = blackjackRoot.querySelector("[data-player-value]");
  const statusNode = blackjackRoot.querySelector("[data-round-status]");
  const messageNode = blackjackRoot.querySelector("[data-round-message]");
  const currentBetNode = blackjackRoot.querySelector("[data-current-bet]");
  const chipStackNode = blackjackRoot.querySelector("[data-chip-stack]");
  let roundToken = "";
  let animationTimer = 0;

  const suitSymbols = {
    hearts: "♥",
    diamonds: "♦",
    clubs: "♣",
    spades: "♠",
  };

  const request = async (url, options = {}) => {
    const response = await fetch(url, {
      headers: {
        "Content-Type": "application/json",
        ...(options.headers || {}),
      },
      ...options,
    });

    if (response.status === 401) {
      window.location.href = `/player/login?returnUrl=${encodeURIComponent(window.location.pathname)}`;
      return null;
    }

    const data = await response.json().catch(() => ({}));

    if (!response.ok) {
      throw new Error(data.message || "Something went wrong.");
    }

    return data;
  };

  const escapeHtml = (value) =>
    String(value || "")
      .replace(/&/g, "&amp;")
      .replace(/</g, "&lt;")
      .replace(/>/g, "&gt;")
      .replace(/"/g, "&quot;")
      .replace(/'/g, "&#039;");

  const renderCards = (node, cards, hideSecondCard) => {
    if (!cards || cards.length === 0) {
      node.innerHTML = `<div class="blackjack-card is-empty">Deal</div>`;
      return;
    }

    node.innerHTML = cards.map((card, index) => {
      if (hideSecondCard && index === 1) {
        return `<div class="blackjack-card is-hidden">?</div>`;
      }

      const suit = escapeHtml(card.suit);
      const rank = escapeHtml(card.rank);
      const symbol = suitSymbols[card.suit] || "";

      return `
        <div class="blackjack-card ${suit}" style="--deal-index: ${index}">
          <span>${rank}</span>
          <strong>${symbol}</strong>
          <span>${rank}</span>
        </div>`;
    }).join("");
  };

  const setBusy = (isBusy) => {
    if (isBusy) {
      dealButton.disabled = true;
      hitButton.disabled = true;
      standButton.disabled = true;
    }
  };

  const renderGame = (game) => {
    roundToken = game.canAct ? game.roundToken : "";
    balanceNode.textContent = game.balance;
    statusNode.textContent = game.status;
    messageNode.textContent = game.message;
    currentBetNode.textContent = game.betAmount;
    if (chipStackNode) {
      const chipCount = Math.max(1, Math.min(5, Math.ceil(Number(game.betAmount || 0) / 10)));
      chipStackNode.innerHTML = Array.from({ length: chipCount }, (_, index) =>
        `<span class="blackjack-chip blackjack-chip-${index % 3}"></span>`).join("");
    }
    playerValueNode.textContent = game.playerValue || 0;
    dealerValueNode.textContent = game.dealerValue === null ? "Hidden" : game.dealerValue;
    betInput.min = game.minBet;
    betInput.max = game.maxBet;
    betInput.disabled = game.canAct;
    dealButton.textContent = game.canAct ? "Round Active" : "Deal";
    dealButton.disabled = game.canAct || game.balance < game.minBet;
    hitButton.disabled = !game.canAct;
    standButton.disabled = !game.canAct;

    renderCards(dealerCardsNode, game.dealerHand, game.canAct);
    renderCards(playerCardsNode, game.playerHand, false);
  };

  const runAction = async (url, body) => {
    setBusy(true);

    try {
      const data = await request(url, {
        method: "POST",
        body: JSON.stringify(body),
      });

      if (data) {
        renderGame(data.game);
        blackjackRoot.classList.remove("is-dealing");
        window.requestAnimationFrame(() => blackjackRoot.classList.add("is-dealing"));
        window.clearTimeout(animationTimer);
        animationTimer = window.setTimeout(() => blackjackRoot.classList.remove("is-dealing"), 360);
      }
    } catch (error) {
      messageNode.textContent = error.message;
    } finally {
      if (!roundToken) {
        dealButton.disabled = false;
      }

      hitButton.disabled = !roundToken;
      standButton.disabled = !roundToken;
    }
  };

  dealButton.addEventListener("click", () => {
    runAction("/blackjack/deal", { betAmount: Number(betInput.value) || 1 });
  });

  hitButton.addEventListener("click", () => {
    if (roundToken) {
      runAction("/blackjack/hit", { roundToken });
    }
  });

  standButton.addEventListener("click", () => {
    if (roundToken) {
      runAction("/blackjack/stand", { roundToken });
    }
  });

  request("/blackjack/state")
    .then((data) => {
      if (data) {
        renderGame(data.game);
      }
    })
    .catch((error) => {
      messageNode.textContent = error.message;
    });
}
