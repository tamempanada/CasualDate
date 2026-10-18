const gachaRoot = document.querySelector("[data-gacha-root]");

if (gachaRoot) {
  const pullButtons = Array.from(gachaRoot.querySelectorAll("[data-gacha-pull]"));
  const refreshButton = gachaRoot.querySelector("[data-gacha-refresh]");
  const statusNode = gachaRoot.querySelector("[data-gacha-status]");
  const revealNode = gachaRoot.querySelector("[data-gacha-reveal]");
  const resultsNode = gachaRoot.querySelector("[data-gacha-results]");
  const collectionNode = gachaRoot.querySelector("[data-gacha-collection]");
  const filterButtons = Array.from(gachaRoot.querySelectorAll("[data-gacha-filter]"));
  const balanceNode = gachaRoot.querySelector("[data-gacha-balance]");
  const pityNode = gachaRoot.querySelector("[data-gacha-pity]");
  const discardDialog = document.querySelector("[data-gacha-discard-dialog]");
  const discardTitle = document.querySelector("[data-discard-title]");
  const discardMessage = document.querySelector("[data-discard-message]");
  const discardConfirm = document.querySelector("[data-discard-confirm]");
  const discardCancel = document.querySelector("[data-discard-cancel]");
  const rewardDialog = document.querySelector("[data-gacha-reward-dialog]");
  const rewardDialogCard = rewardDialog?.querySelector(".gacha-reward-dialog-card");
  const openedReward = document.querySelector("[data-opened-reward]");
  const rewardKeep = document.querySelector("[data-reward-keep]");
  const rewardDiscard = document.querySelector("[data-reward-discard]");
  const rewardClose = document.querySelector("[data-reward-close]");
  let currentFilter = "all";
  let pendingDiscard = null;
  let openedPull = null;
  let openingPullToken = null;
  let pendingPulls = [];
  let collection = [];
  let economy = null;

  const setStatus = (message) => {
    statusNode.textContent = message;
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
      const error = new Error(data.message || "Something went wrong.");
      error.data = data;
      throw error;
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

  const makeRequestToken = () => {
    if (window.crypto?.randomUUID) {
      return window.crypto.randomUUID().replaceAll("-", "");
    }

    return `${Date.now()}${Math.random().toString(16).slice(2)}`;
  };

  const renderEconomy = (state) => {
    economy = state;
    balanceNode.textContent = state?.balance ?? 0;
    pityNode.textContent = `${state?.legendaryPityCount ?? 0}/${state?.legendaryPityThreshold ?? 90}`;

    pullButtons.forEach((button) => {
      const count = Number(button.dataset.gachaPull || "1");
      const cost = count === 10 ? state?.tenPullCost : state?.singlePullCost;
      button.disabled = Number(state?.balance || 0) < Number(cost || 0);
    });
  };

  const loadEconomy = async () => {
    const data = await request("/gacha/state");
    if (data?.economy) {
      renderEconomy(data.economy);
    }
  };

  const showPulls = (data, statusMessage) => {
    if (!data?.hasReward) {
      return false;
    }

    pendingPulls = data.pulls || [];
    resultsNode.innerHTML = pendingPulls.map(renderPullResult).join("");
    revealNode.hidden = false;
    setStatus(statusMessage || `${data.pulls.length} reward${data.pulls.length === 1 ? "" : "s"} revealed. Keep or discard each one.`);
    return true;
  };

  const loadPendingPulls = async () => {
    const data = await request("/gacha/pending");
    if (showPulls(data?.pendingPulls, "Your unresolved rewards are ready to keep or discard.")) {
      await loadEconomy();
    }
  };

  const renderRewardCard = (reward) => {
    const rarityClass = `rarity-${String(reward.rarity || "").toLowerCase()}`;
    const isLetter = reward.rewardType === "Letter";
    const art = isLetter
      ? `<div class="gacha-letter-preview"><strong>${escapeHtml(reward.letterTitle || reward.name)}</strong><p>${escapeHtml(reward.letterContent || reward.description)}</p></div>`
      : reward.imagePath
        ? `<img src="${escapeHtml(reward.imagePath)}" alt="${escapeHtml(reward.name)}" />`
        : `<div class="gacha-art-placeholder">${escapeHtml(reward.rewardType)}</div>`;

    return `
      <article class="gacha-reward-card ${rarityClass}">
        <div class="gacha-art">${art}</div>
        <div class="gacha-card-body">
          <span class="rarity-pill ${rarityClass}">${escapeHtml(reward.rarity)}</span>
          <h3>${escapeHtml(reward.name)}</h3>
          <p>${escapeHtml(reward.description)}</p>
          <small>${escapeHtml(reward.rewardType)}${reward.quantity > 1 ? ` x${reward.quantity}` : ""}</small>
        </div>
      </article>`;
  };

  const renderPullResult = (pull) => {
    const rarityClass = `rarity-${String(pull.reward.rarity || "").toLowerCase()}`;
    return `
      <button class="gacha-capsule ${rarityClass}" type="button" data-gacha-open="${escapeHtml(pull.pullToken)}" aria-label="Open reward capsule">
        <span class="gacha-capsule-shine"></span>
        <span class="gacha-capsule-seal">?</span>
      </button>`;
  };

  const renderCollection = () => {
    if (!collectionNode) {
      return;
    }

    const visibleRewards = currentFilter === "all"
      ? collection
      : collection.filter((reward) => reward.rewardType === currentFilter);

    collectionNode.innerHTML = visibleRewards.length === 0
      ? `<p class="soft-copy">No kept rewards here yet.</p>`
      : visibleRewards.map(renderRewardCard).join("");
  };

  const loadCollection = async () => {
    if (!collectionNode) {
      return;
    }

    collectionNode.innerHTML = `<p class="soft-copy">Loading collection...</p>`;
    const data = await request("/gacha/collection");
    if (data) {
      collection = data.rewards || [];
      renderCollection();
    }
  };

  const resolvePull = async (pullToken, keep) => {
    const capsule = resultsNode.querySelector(`[data-gacha-open="${CSS.escape(pullToken)}"]`);
    if (capsule) {
      capsule.disabled = true;
    }

    const data = await request(keep ? "/gacha/keep" : "/gacha/trash", {
      method: "POST",
      body: JSON.stringify({ pullToken }),
    });

    if (!data) {
      return;
    }

    setStatus(keep
      ? "Reward kept in your permanent collection."
      : `Reward discarded. +${data.refundAmount || 0} tokens refunded.`);

    if (typeof data.balance === "number" && economy) {
      renderEconomy({ ...economy, balance: data.balance });
    } else {
      await loadEconomy();
    }

    await loadCollection();
    capsule?.remove();
    pendingPulls = pendingPulls.filter((pull) => pull.pullToken !== pullToken);
    if (!resultsNode.querySelector("[data-gacha-open]")) {
      revealNode.hidden = true;
    }
  };

  const openDiscardDialog = (pull) => {
    pendingDiscard = pull;
    discardTitle.textContent = `Discard ${pull.reward.name}?`;
    discardMessage.textContent = `${pull.reward.rarity} reward. Confirm discard to receive ${pull.refundAmount} token${pull.refundAmount === 1 ? "" : "s"}.`;
    discardDialog.hidden = false;
  };

  const closeDiscardDialog = () => {
    const capsule = resultsNode.querySelector(`[data-gacha-open="${CSS.escape(pendingDiscard?.pullToken || "")}"]`);
    if (capsule) {
      capsule.disabled = false;
      capsule.classList.remove("is-opening");
    }
    pendingDiscard = null;
    discardDialog.hidden = true;
  };

  const openRewardDialog = (pull) => {
    openingPullToken = null;
    openedPull = pull;
    rewardKeep.disabled = false;
    rewardDiscard.disabled = false;
    openedReward.innerHTML = renderRewardCard(pull.reward);
    rewardDiscard.textContent = `Discard +${pull.refundAmount} tokens`;
    rewardDialogCard?.classList.remove("is-epic", "is-legendary");
    const rarity = String(pull.reward.rarity || "").toLowerCase();
    if (rarity === "epic" || rarity === "legendary") {
      rewardDialogCard?.classList.add(`is-${rarity}`);
    }
    rewardDialog.hidden = false;
  };

  const closeRewardDialog = (keepCapsuleLocked = false) => {
    const capsule = resultsNode.querySelector(`[data-gacha-open="${CSS.escape(openedPull?.pullToken || "")}"]`);
    if (capsule && !keepCapsuleLocked) {
      capsule.disabled = false;
      capsule.classList.remove("is-opening");
    }
    openedPull = null;
    rewardDialogCard?.classList.remove("is-epic", "is-legendary");
    rewardDialog.hidden = true;
  };

  pullButtons.forEach((button) => {
    button.addEventListener("click", async () => {
      const pullCount = Number(button.dataset.gachaPull || "1");
      pullButtons.forEach((item) => {
        item.disabled = true;
      });
      revealNode.hidden = true;
      resultsNode.innerHTML = "";
      setStatus(pullCount === 10 ? "Turning ten capsules..." : "Turning one capsule...");

      try {
        const data = await request("/gacha/pull", {
          method: "POST",
          body: JSON.stringify({ pullCount, requestToken: makeRequestToken() }),
        });

        if (!data?.hasReward) {
          setStatus(data?.message || "No rewards available.");
          return;
        }

        showPulls(data);
        await loadEconomy();
      } catch (error) {
        setStatus(error.data?.missingTokens ? `${error.message} Balance: ${error.data.balance}.` : error.message);
      } finally {
        if (economy) {
          renderEconomy(economy);
        }
      }
    });
  });

  resultsNode.addEventListener("click", async (event) => {
    const pullToken = event.target.closest("[data-gacha-open]")?.dataset.gachaOpen;
    if (!pullToken) {
      return;
    }

    const pull = resultsNode.querySelector(`[data-gacha-open="${CSS.escape(pullToken)}"]`);
    const result = pendingPulls.find((item) => item.pullToken === pullToken);
    if (pull && result && !openedPull && !openingPullToken && !pendingDiscard) {
      openingPullToken = pullToken;
      pull.disabled = true;
      pull.classList.add("is-opening");
      window.setTimeout(() => openRewardDialog(result), 180);
    }
  });

  rewardKeep.addEventListener("click", async () => {
    if (!openedPull) {
      return;
    }

    rewardKeep.disabled = true;
    rewardDiscard.disabled = true;
    try {
      await resolvePull(openedPull.pullToken, true);
      closeRewardDialog();
    } catch (error) {
      setStatus(error.message);
      rewardKeep.disabled = false;
      rewardDiscard.disabled = false;
    }
  });

  rewardDiscard.addEventListener("click", () => {
    if (!openedPull) {
      return;
    }

    openDiscardDialog(openedPull);
    closeRewardDialog(true);
  });

  rewardClose.addEventListener("click", closeRewardDialog);

  discardConfirm.addEventListener("click", async () => {
    if (!pendingDiscard) {
      return;
    }

    discardConfirm.disabled = true;
    try {
      await resolvePull(pendingDiscard.pullToken, false);
      closeDiscardDialog();
    } catch (error) {
      setStatus(error.message);
    } finally {
      discardConfirm.disabled = false;
    }
  });

  discardCancel.addEventListener("click", closeDiscardDialog);
  refreshButton?.addEventListener("click", loadCollection);

  filterButtons.forEach((button) => {
    button.addEventListener("click", () => {
      currentFilter = button.dataset.gachaFilter || "all";
      filterButtons.forEach((item) => item.classList.toggle("is-active", item === button));
      renderCollection();
    });
  });

  Promise.all([loadEconomy(), loadCollection(), loadPendingPulls()]).catch((error) => {
    setStatus(error.message);
  });
}
