const customizeRoot = document.querySelector("[data-customize-root]");

if (customizeRoot) {
  const grid = customizeRoot.querySelector("[data-customize-grid]");
  const status = customizeRoot.querySelector("[data-customize-status]");
  const tabs = Array.from(customizeRoot.querySelectorAll("[data-customize-tab]"));
  let state = null;
  let category = "Theme";

  const escapeHtml = (value) => String(value || "")
    .replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;")
    .replace(/"/g, "&quot;").replace(/'/g, "&#039;");

  const request = async (url, options = {}) => {
    const response = await fetch(url, {
      headers: { "Content-Type": "application/json", ...(options.headers || {}) },
      ...options,
    });
    const data = await response.json().catch(() => ({}));
    if (!response.ok) throw new Error(data.message || "That change could not be saved.");
    return data;
  };

  const cosmeticCard = (item) => {
    const previewAction = item.isOwned && item.category === "Theme"
      ? `<button class="secondary-button" type="button" data-preview-theme="${escapeHtml(item.styleKey)}">Preview</button>`
      : "";
    const action = item.isOwned
      ? item.isEquipped
        ? `<button class="secondary-button" type="button" data-unequip="${escapeHtml(item.category)}">Unequip</button>`
        : `<button class="primary-button" type="button" data-equip="${escapeHtml(item.id)}" data-category="${escapeHtml(item.category)}" data-style-key="${escapeHtml(item.styleKey)}">Equip</button>`
      : `<span class="customize-locked">Locked in Gacha</span>`;
    const preview = item.imagePath
      ? `<img src="${escapeHtml(item.imagePath)}" alt="" />`
      : `<div class="cosmetic-swatch cosmetic-${escapeHtml(item.styleKey)}" aria-hidden="true"></div>`;
    return `<article class="customize-card ${item.isOwned ? "is-owned" : ""}">
      <div class="customize-preview">${preview}</div>
      <div><span class="rarity-pill rarity-legendary">Legendary</span><h2>${escapeHtml(item.name)}</h2><p>${escapeHtml(item.description)}</p></div>
      <div class="customize-card-footer">${item.isEquipped ? '<span class="customize-equipped">Equipped</span>' : ""}${previewAction}${action}</div>
    </article>`;
  };

  const companionCard = (companion) => {
    const preview = companion.imagePath
      ? `<img src="${escapeHtml(companion.imagePath)}" alt="" />`
      : `<div class="cosmetic-swatch" aria-hidden="true">Doll</div>`;
    return `<article class="customize-card companion-card ${companion.isEquipped ? "is-owned" : ""}">
      <div class="customize-preview ${companion.rarity === "Legendary" ? "is-legendary" : ""}">${preview}</div>
      <div><span class="rarity-pill rarity-${escapeHtml(companion.rarity.toLowerCase())}">${escapeHtml(companion.rarity)}</span><h2>${escapeHtml(companion.name)}</h2><p>${escapeHtml(companion.description)}</p></div>
      <div class="customize-card-footer">${companion.isEquipped ? '<span class="customize-equipped">Equipped</span><button class="secondary-button" type="button" data-companion="">Unequip</button>' : `<button class="primary-button" type="button" data-companion="${companion.rewardId}">Choose companion</button>`}</div>
    </article>`;
  };

  const render = () => {
    if (!state) return;
    if (category === "Companion") {
      grid.innerHTML = state.companions.length
        ? state.companions.map(companionCard).join("")
        : `<p class="customize-empty">Keep a Doll reward from Gacha to choose a companion.</p>`;
      return;
    }
    const visible = state.cosmetics.filter((item) => item.category === category);
    const trackPicker = category === "MusicSkin"
      ? `<article class="customize-card customize-track-card"><div><span class="rarity-pill">Available music</span><h2>Preferred track</h2><p>Choose a valid track for the persistent player. The current song keeps playing until you change screens.</p></div><label class="customize-track-select">Track<select data-track-select><option value="">Use the admin default</option>${state.tracks.map((track) => `<option value="${track.id}" ${track.isSelected ? "selected" : ""}>${escapeHtml(track.name)}</option>`).join("")}</select></label><div class="customize-card-footer"><button class="primary-button" type="button" data-save-track>Save track</button></div></article>`
      : "";
    grid.innerHTML = visible.map(cosmeticCard).join("") + trackPicker;
  };

  const reload = async () => {
    state = await request("/customize/state");
    render();
  };

  tabs.forEach((tab) => tab.addEventListener("click", () => {
    category = tab.dataset.customizeTab;
    tabs.forEach((item) => item.classList.toggle("is-active", item === tab));
    render();
  }));

  grid.addEventListener("click", async (event) => {
    const equip = event.target.closest("[data-equip]");
    const unequip = event.target.closest("[data-unequip]");
    const companion = event.target.closest("[data-companion]");
    const previewTheme = event.target.closest("[data-preview-theme]");
    const saveTrack = event.target.closest("[data-save-track]");
    try {
      if (saveTrack) {
        saveTrack.disabled = true;
        const selectedTrack = grid.querySelector("[data-track-select]")?.value;
        await request("/customize/track", { method: "POST", body: JSON.stringify({ mediaId: selectedTrack ? Number(selectedTrack) : null }) });
        status.textContent = "Preferred track saved. It will be selected on your next screen without interrupting the current song.";
      } else if (previewTheme) {
        document.body.dataset.theme = previewTheme.dataset.previewTheme;
        status.textContent = "Previewing this theme. Equip it to keep it after you leave this screen.";
        return;
      } else if (equip || unequip) {
        const button = equip || unequip;
        button.disabled = true;
        const cosmeticCategory = button.dataset.category || button.dataset.unequip;
        await request("/customize/equip", { method: "POST", body: JSON.stringify({ category: cosmeticCategory, cosmeticId: equip?.dataset.equip || null }) });
        const styleKey = equip?.dataset.styleKey || (cosmeticCategory === "Theme" ? "default" : cosmeticCategory === "MusicSkin" ? "default" : "none");
        if (cosmeticCategory === "Theme") {
          document.body.dataset.theme = styleKey;
        } else if (cosmeticCategory === "MusicSkin") {
          const musicWidget = document.querySelector("[data-music-widget]");
          if (musicWidget) {
            [...musicWidget.classList]
              .filter((className) => className.startsWith("music-skin-"))
              .forEach((className) => musicWidget.classList.remove(className));
            musicWidget.classList.add(`music-skin-${styleKey}`);
          }
        } else if (cosmeticCategory === "Effect") {
          document.body.dataset.effect = styleKey;
        }
        status.textContent = equip ? "Equipped. Your style is updated." : "Default style restored.";
      } else if (companion) {
        companion.disabled = true;
        const value = companion.dataset.companion;
        await request("/customize/companion", { method: "POST", body: JSON.stringify({ rewardId: value ? Number(value) : null }) });
        status.textContent = value ? "Companion selected." : "Companion removed.";
      } else { return; }
      await reload();
    } catch (error) {
      status.textContent = error.message;
      event.target.disabled = false;
    }
  });

  reload().then(() => { status.textContent = "Choose an owned Legendary cosmetic to equip."; }).catch((error) => { status.textContent = error.message; });
}
