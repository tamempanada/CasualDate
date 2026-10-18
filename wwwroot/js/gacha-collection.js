const collectionRoot = document.querySelector("[data-gacha-collection-root]");

if (collectionRoot) {
  const collectionNode = collectionRoot.querySelector("[data-gacha-collection]");
  const refreshButton = collectionRoot.querySelector("[data-gacha-refresh]");
  const statusNode = collectionRoot.querySelector("[data-gacha-status]");
  const filterButtons = Array.from(collectionRoot.querySelectorAll("[data-gacha-filter]"));
  let collection = [];
  let typeFilter = "all";
  let rarityFilter = "all";

  const setStatus = (message) => {
    statusNode.textContent = message;
  };

  const request = async (url) => {
    const response = await fetch(url, {
      headers: { "Content-Type": "application/json" },
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

  const renderCollection = () => {
    const visibleRewards = collection.filter((reward) => {
      const matchesType = typeFilter === "all" || reward.rewardType === typeFilter;
      const matchesRarity = rarityFilter === "all" || reward.rarity === rarityFilter;
      return matchesType && matchesRarity;
    });

    collectionNode.innerHTML = visibleRewards.length === 0
      ? `<p class="soft-copy">No kept rewards match this filter yet.</p>`
      : visibleRewards.map(renderRewardCard).join("");

    setStatus(`${visibleRewards.length} reward${visibleRewards.length === 1 ? "" : "s"} shown.`);
  };

  const loadCollection = async () => {
    setStatus("Loading collection...");
    collectionNode.innerHTML = `<p class="soft-copy">Loading collection...</p>`;

    const data = await request("/gacha/collection");
    if (!data) {
      return;
    }

    collection = data.rewards || [];
    renderCollection();
  };

  filterButtons.forEach((button) => {
    button.addEventListener("click", () => {
      const group = button.dataset.gachaFilterGroup || "type";
      const value = button.dataset.gachaFilter || "all";

      if (group === "rarity") {
        rarityFilter = value;
      } else {
        typeFilter = value;
      }

      filterButtons
        .filter((item) => (item.dataset.gachaFilterGroup || "type") === group)
        .forEach((item) => item.classList.toggle("is-active", item === button));
      renderCollection();
    });
  });

  refreshButton.addEventListener("click", loadCollection);

  loadCollection().catch((error) => {
    collectionNode.innerHTML = `<p class="soft-copy">Collection could not load.</p>`;
    setStatus(error.message);
  });
}
