const musicWidget = document.querySelector("[data-music-widget]");
const musicAudio = document.querySelector("[data-music-audio]");
const musicToggle = document.querySelector("[data-music-toggle]");
const musicSeek = document.querySelector("[data-music-seek]");
const musicTime = document.querySelector("[data-music-time]");
const musicTitle = document.querySelector("[data-music-title]");
const musicCover = document.querySelector("[data-music-cover]");
const musicCoverPlaceholder = document.querySelector("[data-music-cover-placeholder]");
const musicPrev = document.querySelector("[data-music-prev]");
const musicNext = document.querySelector("[data-music-next]");
const musicLoop = document.querySelector("[data-music-loop]");
const musicPickerToggle = document.querySelector("[data-music-picker-toggle]");
const musicPicker = document.querySelector("[data-music-picker]");
const musicLibrary = document.querySelector("[data-music-library]");
const musicQueue = document.querySelector("[data-music-queue]");
const musicLibraryJson = document.querySelector("[data-music-library-json]");
const musicMinimize = document.querySelector("[data-music-minimize]");
const musicExpand = document.querySelector("[data-music-expand]");
const musicPreferenceKey = "casual-date-music-enabled";
const musicQueueKey = "casual-date-music-queue";
const musicLoopKey = "casual-date-music-loop";
const musicMinimizedKey = "casual-date-music-minimized";
const musicCurrentTrackKey = "casual-date-music-current-track";
let musicTracks = [];
let currentMusicTrackId = Number(musicWidget?.dataset.musicId || 0);
let musicQueueTrackIds = [];
let isMusicLooping = window.localStorage.getItem(musicLoopKey) === "true";
let isMusicMinimized = window.localStorage.getItem(musicMinimizedKey) === "true";
let musicTimeSaveTimer = null;

document.addEventListener("keydown", (event) => {
  const key = event.key.toLowerCase();

  if (event.ctrlKey && event.altKey && key === "a") {
    window.location.href = "/admin/login";
  }
});

function isPlayerNavigationPath(url) {
  return (url.pathname === "/Home/Menu" || url.pathname.startsWith("/Game/") || url.pathname.startsWith("/customize"))
    && !url.pathname.startsWith("/Game/Index");
}

function canUsePlayerNavigation(url) {
  return Boolean(musicWidget)
    && url.origin === window.location.origin
    && isPlayerNavigationPath(url);
}

async function runPageScript(script) {
  const scriptType = (script.getAttribute("type") || "text/javascript").toLowerCase();

  if (!["", "text/javascript", "application/javascript", "module"].includes(scriptType)) {
    return;
  }

  if (script.src) {
    const sourceUrl = new URL(script.getAttribute("src"), window.location.href);

    if (sourceUrl.pathname.endsWith("/js/site.js") || !sourceUrl.pathname.startsWith("/js/")) {
      return;
    }

    const response = await fetch(sourceUrl.href, { cache: "force-cache" });

    if (!response.ok) {
      throw new Error(`Unable to load ${sourceUrl.pathname}`);
    }

    const code = await response.text();
    const executable = document.createElement("script");
    executable.dataset.dynamicPageScript = "true";
    executable.textContent = `(function () {\n${code}\n})();\n//# sourceURL=${sourceUrl.pathname}`;
    document.body.appendChild(executable);
    return;
  }

  if (script.textContent.trim()) {
    const executable = document.createElement("script");
    executable.dataset.dynamicPageScript = "true";
    executable.textContent = `(function () {\n${script.textContent}\n})();`;
    document.body.appendChild(executable);
  }
}

async function runPageScripts(nextDocument) {
  document.querySelectorAll("[data-dynamic-page-script]").forEach((script) => script.remove());

  const scripts = Array.from(nextDocument.body.querySelectorAll("script"));

  for (const script of scripts) {
    await runPageScript(script);
  }
}

async function navigateWithinPlayer(url, options = {}) {
  if (!canUsePlayerNavigation(url)) {
    window.location.href = url.href;
    return;
  }

  const pageFrame = document.querySelector(".page-frame");
  const currentMain = document.querySelector("main[role='main']");

  if (!pageFrame || !currentMain) {
    window.location.href = url.href;
    return;
  }

  try {
    window.dispatchEvent(new CustomEvent("casual-date:before-page-swap"));
    pageFrame.classList.add("is-navigating");

    const response = await fetch(url.href, {
      headers: { "X-Requested-With": "fetch" }
    });

    if (!response.ok) {
      throw new Error(`Navigation failed: ${response.status}`);
    }

    const html = await response.text();
    const nextDocument = new DOMParser().parseFromString(html, "text/html");
    const nextMain = nextDocument.querySelector("main[role='main']");

    if (!nextMain) {
      throw new Error("Navigation response did not include page content.");
    }

    document.title = nextDocument.title;
    document.body.className = nextDocument.body.className;
    document.body.dataset.theme = nextDocument.body.dataset.theme || "default";
    document.body.dataset.effect = nextDocument.body.dataset.effect || "none";
    document.body.setAttribute("style", nextDocument.body.getAttribute("style") || "");
    const nextMusicWidget = nextDocument.querySelector("[data-music-widget]");
    if (musicWidget && nextMusicWidget) {
      const nextSkinClass = [...nextMusicWidget.classList].find(className => className.startsWith("music-skin-"));
      [...musicWidget.classList]
        .filter(className => className.startsWith("music-skin-"))
        .forEach(className => musicWidget.classList.remove(className));
      if (nextSkinClass) {
        musicWidget.classList.add(nextSkinClass);
      }

      const companionImage = musicWidget.querySelector("[data-music-companion]");
      const companionArtwork = musicWidget.querySelector(".music-artwork");
      const companionSrc = nextMusicWidget.dataset.companionSrc || "";
      const companionName = nextMusicWidget.dataset.companionName || "";
      musicWidget.dataset.companionSrc = companionSrc;
      musicWidget.dataset.companionName = companionName;
      if (companionImage) {
        companionImage.src = companionSrc;
        companionImage.alt = companionName ? `${companionName}, your companion doll` : "";
        companionImage.classList.toggle("is-hidden", !companionSrc);
      }
      companionArtwork?.classList.toggle("has-companion", Boolean(companionSrc));
    }
    currentMain.innerHTML = nextMain.innerHTML;

    if (!options.replaceHistory) {
      window.history.pushState({ playerNavigation: true }, "", url.href);
    }

    await runPageScripts(nextDocument);
    window.scrollTo({ top: 0, behavior: "auto" });
  } catch {
    window.location.href = url.href;
  } finally {
    window.setTimeout(() => pageFrame.classList.remove("is-navigating"), 80);
  }
}

window.CasualDateNavigate = (targetUrl, options = {}) => {
  const url = new URL(targetUrl, window.location.href);
  return navigateWithinPlayer(url, options);
};

document.addEventListener("click", (event) => {
  const link = event.target.closest("a[href]");

  if (!link || event.defaultPrevented || event.button !== 0 || event.metaKey || event.ctrlKey || event.shiftKey || event.altKey) {
    return;
  }

  if (link.target || link.hasAttribute("download")) {
    return;
  }

  const url = new URL(link.href, window.location.href);

  if (!canUsePlayerNavigation(url)) {
    return;
  }

  event.preventDefault();
  navigateWithinPlayer(url);
});

window.addEventListener("popstate", () => {
  const url = new URL(window.location.href);

  if (canUsePlayerNavigation(url)) {
    navigateWithinPlayer(url, { replaceHistory: true });
  }
});

if (musicLibraryJson) {
  try {
    musicTracks = JSON.parse(musicLibraryJson.textContent || "[]");
  } catch {
    musicTracks = [];
  }
}

try {
  musicQueueTrackIds = JSON.parse(window.localStorage.getItem(musicQueueKey) || "[]")
    .map((id) => Number(id))
    .filter((id) => musicTracks.some((track) => track.id === id));
} catch {
  musicQueueTrackIds = [];
}

function getMusicTimeKey(trackId = currentMusicTrackId) {
  return `casual-date-music-time-${trackId || "default"}`;
}

function getCurrentMusicTrack() {
  return musicTracks.find((track) => track.id === currentMusicTrackId) || musicTracks[0] || null;
}

function getSavedMusicTrackId() {
  const savedTrackId = Number(window.localStorage.getItem(musicCurrentTrackKey) || 0);
  return musicTracks.some((track) => track.id === savedTrackId) ? savedTrackId : null;
}

function saveCurrentMusicTrack() {
  if (currentMusicTrackId) {
    window.localStorage.setItem(musicCurrentTrackKey, String(currentMusicTrackId));
  }
}

function updateMusicButton(isPlaying) {
  if (!musicToggle) {
    return;
  }

  musicToggle.textContent = isPlaying ? "Pause" : "Play";
  musicToggle.setAttribute("aria-pressed", String(isPlaying));
}

function updateMusicLoopButton() {
  if (!musicLoop || !musicAudio) {
    return;
  }

  musicAudio.loop = isMusicLooping;
  musicLoop.setAttribute("aria-pressed", String(isMusicLooping));
  musicLoop.textContent = isMusicLooping ? "Loop on" : "Loop";
}

function updateMusicMinimizedState() {
  if (!musicWidget || !musicExpand) {
    return;
  }

  musicWidget.classList.toggle("is-minimized", isMusicMinimized);
  musicExpand.hidden = !isMusicMinimized;

  if (isMusicMinimized && musicPicker) {
    musicPicker.hidden = true;
    musicPickerToggle?.setAttribute("aria-expanded", "false");
  }
}

function formatMusicTime(seconds) {
  if (!Number.isFinite(seconds) || seconds < 0) {
    return "0:00";
  }

  const totalSeconds = Math.floor(seconds);
  const minutes = Math.floor(totalSeconds / 60);
  const remainingSeconds = String(totalSeconds % 60).padStart(2, "0");

  return `${minutes}:${remainingSeconds}`;
}

function saveMusicQueue() {
  window.localStorage.setItem(musicQueueKey, JSON.stringify(musicQueueTrackIds));
}

function renderMusicQueue() {
  if (!musicQueue) {
    return;
  }

  if (musicQueueTrackIds.length === 0) {
    musicQueue.textContent = "Queue is empty.";
    return;
  }

  musicQueue.textContent = "";
  musicQueueTrackIds.forEach((trackId, index) => {
    const track = musicTracks.find((item) => item.id === trackId);

    if (!track) {
      return;
    }

    const item = document.createElement("div");
    item.className = "music-queue-item";
    item.textContent = `${index + 1}. ${track.title}`;
    musicQueue.appendChild(item);
  });
}

function renderMusicLibrary() {
  if (!musicLibrary) {
    return;
  }

  musicLibrary.textContent = "";
  musicTracks.forEach((track) => {
    const row = document.createElement("div");
    row.className = "music-track-row";
    row.dataset.trackId = String(track.id);

    const cover = document.createElement(track.coverSrc ? "img" : "span");
    cover.className = track.coverSrc ? "music-track-cover" : "music-track-cover music-track-cover-empty";

    if (track.coverSrc) {
      cover.src = track.coverSrc;
      cover.alt = "";
    } else {
      cover.textContent = "Music";
    }

    const title = document.createElement("strong");
    title.textContent = track.title;

    const playButton = document.createElement("button");
    playButton.type = "button";
    playButton.textContent = "Play";
    playButton.addEventListener("click", () => {
      loadMusicTrack(track.id, { autoplay: true });
    });

    const queueButton = document.createElement("button");
    queueButton.type = "button";
    queueButton.textContent = "Add";
    queueButton.addEventListener("click", () => {
      musicQueueTrackIds.push(track.id);
      saveMusicQueue();
      renderMusicQueue();
    });

    row.append(cover, title, playButton, queueButton);
    musicLibrary.appendChild(row);
  });
}

function updateMusicArtwork(track) {
  if (musicTitle) {
    musicTitle.textContent = track?.title || "Music";
  }

  if (!musicCover || !musicCoverPlaceholder) {
    return;
  }

  if (track?.coverSrc) {
    musicCover.src = track.coverSrc;
    musicCover.classList.remove("is-hidden");
    musicCoverPlaceholder.classList.add("is-hidden");
    return;
  }

  musicCover.removeAttribute("src");
  musicCover.classList.add("is-hidden");
  musicCoverPlaceholder.classList.remove("is-hidden");
}

function updateMusicProgress() {
  if (!musicAudio) {
    return;
  }

  const duration = Number.isFinite(musicAudio.duration) ? musicAudio.duration : 0;
  const currentTime = Number.isFinite(musicAudio.currentTime) ? musicAudio.currentTime : 0;

  if (musicSeek) {
    musicSeek.max = String(Math.max(1, Math.floor(duration)));
    musicSeek.value = String(Math.min(Math.floor(currentTime), Number(musicSeek.max)));
  }

  if (musicTime) {
    musicTime.textContent = `${formatMusicTime(currentTime)} / ${formatMusicTime(duration)}`;
  }
}

function restoreMusicTime(trackId = currentMusicTrackId) {
  if (!musicAudio || musicAudio.readyState === 0) {
    return;
  }

  const savedTime = Number(window.localStorage.getItem(getMusicTimeKey(trackId)) || 0);

  if (savedTime > 0 && Number.isFinite(savedTime)) {
    musicAudio.currentTime = Math.min(savedTime, musicAudio.duration || savedTime);
  }
}

function saveMusicTime() {
  if (!musicAudio || !Number.isFinite(musicAudio.currentTime)) {
    return;
  }

  window.localStorage.setItem(getMusicTimeKey(), String(Math.floor(musicAudio.currentTime)));
  updateMusicProgress();
}

function loadMusicTrack(trackId, options = {}) {
  if (!musicAudio || !musicWidget) {
    return;
  }

  const track = musicTracks.find((item) => item.id === Number(trackId));

  if (!track) {
    return;
  }

  if (!options.skipSaveCurrentTime) {
    saveMusicTime();
  }
  currentMusicTrackId = track.id;
  saveCurrentMusicTrack();
  musicWidget.dataset.musicId = String(track.id);
  if (musicAudio.getAttribute("src") !== track.src) {
    musicAudio.src = track.src;
  }
  updateMusicArtwork(track);
  updateMusicProgress();

  const onMetadata = () => {
    restoreMusicTime(track.id);
    updateMusicProgress();
    musicAudio.removeEventListener("loadedmetadata", onMetadata);
  };

  musicAudio.addEventListener("loadedmetadata", onMetadata);

  if (options.autoplay) {
    playMusic();
  }
}

async function playMusic() {
  if (!musicAudio) {
    return;
  }

  try {
    restoreMusicTime();
    saveCurrentMusicTrack();
    await musicAudio.play();
    window.localStorage.setItem(musicPreferenceKey, "true");
    updateMusicButton(true);
    updateMusicProgress();
  } catch {
    window.localStorage.setItem(musicPreferenceKey, "false");
    updateMusicButton(false);
  }
}

function pauseMusic() {
  if (!musicAudio) {
    return;
  }

  musicAudio.pause();
  saveCurrentMusicTrack();
  window.localStorage.setItem(musicPreferenceKey, "false");
  saveMusicTime();
  updateMusicProgress();
  updateMusicButton(false);
}

function startSavingMusicTime() {
  if (musicTimeSaveTimer) {
    return;
  }

  musicTimeSaveTimer = window.setInterval(saveMusicTime, 1000);
}

function getNextLibraryTrackId(direction = 1) {
  if (musicTracks.length === 0) {
    return null;
  }

  const currentIndex = Math.max(0, musicTracks.findIndex((track) => track.id === currentMusicTrackId));
  const nextIndex = (currentIndex + direction + musicTracks.length) % musicTracks.length;

  return musicTracks[nextIndex].id;
}

function playNextMusicTrack() {
  const wasPlaying = musicAudio && !musicAudio.paused;
  const queuedTrackId = musicQueueTrackIds.shift();
  const nextTrackId = queuedTrackId || getNextLibraryTrackId(1);

  saveMusicQueue();
  renderMusicQueue();

  if (nextTrackId) {
    loadMusicTrack(nextTrackId, { autoplay: wasPlaying });
  }
}

function playPreviousMusicTrack() {
  const wasPlaying = musicAudio && !musicAudio.paused;
  const previousTrackId = getNextLibraryTrackId(-1);

  if (previousTrackId) {
    loadMusicTrack(previousTrackId, { autoplay: wasPlaying });
  }
}

musicToggle?.addEventListener("click", () => {
  if (musicAudio.paused) {
    playMusic();
    return;
  }

  pauseMusic();
});

musicPrev?.addEventListener("click", playPreviousMusicTrack);
musicNext?.addEventListener("click", playNextMusicTrack);

musicLoop?.addEventListener("click", () => {
  isMusicLooping = !isMusicLooping;
  window.localStorage.setItem(musicLoopKey, String(isMusicLooping));
  updateMusicLoopButton();
});

musicPickerToggle?.addEventListener("click", () => {
  if (!musicPicker || !musicPickerToggle) {
    return;
  }

  const isOpen = !musicPicker.hidden;
  musicPicker.hidden = isOpen;
  musicPickerToggle.setAttribute("aria-expanded", String(!isOpen));
});

musicMinimize?.addEventListener("click", () => {
  isMusicMinimized = true;
  window.localStorage.setItem(musicMinimizedKey, "true");
  updateMusicMinimizedState();
});

musicExpand?.addEventListener("click", () => {
  isMusicMinimized = false;
  window.localStorage.setItem(musicMinimizedKey, "false");
  updateMusicMinimizedState();
});

musicSeek?.addEventListener("input", () => {
  if (!musicAudio) {
    return;
  }

  musicAudio.currentTime = Number(musicSeek.value);
  saveMusicTime();
});

musicAudio?.addEventListener("loadedmetadata", () => {
  restoreMusicTime();
  updateMusicProgress();
});

musicAudio?.addEventListener("ended", () => {
  if (!isMusicLooping) {
    playNextMusicTrack();
  }
});
musicAudio?.addEventListener("play", startSavingMusicTime);
musicAudio?.addEventListener("pause", saveMusicTime);
musicAudio?.addEventListener("timeupdate", updateMusicProgress);
window.addEventListener("pagehide", () => {
  saveCurrentMusicTrack();
  saveMusicTime();
});

if (musicAudio && musicTracks.length > 0) {
  window.localStorage.removeItem("casual-date-music-volume");
  musicAudio.muted = false;
  musicAudio.volume = 1;

  const preferredTrackId = Number(musicWidget.dataset.preferredTrack || 0);
  const savedTrackId = preferredTrackId || getSavedMusicTrackId();

  if (savedTrackId && savedTrackId !== currentMusicTrackId) {
    loadMusicTrack(savedTrackId, { skipSaveCurrentTime: true });
  } else {
    saveCurrentMusicTrack();
  }
}

updateMusicArtwork(getCurrentMusicTrack());
updateMusicLoopButton();
updateMusicMinimizedState();
renderMusicLibrary();
renderMusicQueue();
updateMusicProgress();

if (musicWidget && musicAudio && window.localStorage.getItem(musicPreferenceKey) === "true") {
  playMusic();
}

const reducedMotion = window.matchMedia("(prefers-reduced-motion: reduce)").matches;

if (!reducedMotion) {
  const addParticle = (x, y, kind) => {
    const particle = document.createElement("span");
    particle.className = `cosmetic-particle cosmetic-particle-${kind}`;
    particle.style.left = `${x}px`;
    particle.style.top = `${y}px`;
    document.body.append(particle);
    window.setTimeout(() => particle.remove(), 720);
  };

  let lastTrail = 0;
  document.addEventListener("pointermove", (event) => {
    const activeEffect = document.body.dataset.effect || "none";
    if ((activeEffect !== "rose-petals" && activeEffect !== "stardust") || event.pointerType === "touch" || Date.now() - lastTrail < 60) return;
    lastTrail = Date.now();
    addParticle(event.clientX, event.clientY, activeEffect);
  }, { passive: true });

  document.addEventListener("pointerdown", (event) => {
    const activeEffect = document.body.dataset.effect || "none";
    if (event.target.closest("input, textarea, select")) return;
    if (activeEffect === "heartbeats" || activeEffect === "golden-touch") addParticle(event.clientX, event.clientY, activeEffect);
    if (activeEffect === "dream-sparkles" && event.target.closest("button, a, .game-card")) addParticle(event.clientX, event.clientY, activeEffect);
  }, { passive: true });
}
