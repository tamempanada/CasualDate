const yesButton = document.querySelector("#yesButton");
const noButton = document.querySelector("#noButton");
const noMessage = document.querySelector("#noMessage");
const invitationScreen = document.querySelector(".invitation-screen");
const invitationCard = document.querySelector(".invitation-card");

let noClicks = 0;
let noButtonConverted = false;

if (yesButton && noButton && invitationScreen) {
  const clickLimit = Number(invitationScreen.dataset.noDisableAfter || 8);
  const growthRate = Number(invitationScreen.dataset.yesGrowthRate || 0.28);
  const messages = [
    "Are you sure?",
    "I prepared snacks though.",
    "The yes button is getting ideas.",
    "This date is very persistent.",
    "Last chance to escape gracefully."
  ];

  noButton.addEventListener("click", () => {
    if (noButtonConverted) {
      window.location.href = yesButton.href;
      return;
    }

    noClicks += 1;
    const yesScale = Math.min(1 + noClicks * growthRate, 2.15);
    const yesWidth = Math.min(120 + noClicks * growthRate * 135, 300);
    const yesHeight = Math.min(48 + noClicks * growthRate * 18, 72);
    const yesFontSize = Math.min(1 + noClicks * growthRate * 0.08, 1.18);

    yesButton.style.setProperty("--yes-scale", String(yesScale));
    yesButton.style.setProperty("--yes-width", `${yesWidth}px`);
    yesButton.style.setProperty("--yes-height", `${yesHeight}px`);
    yesButton.style.setProperty("--yes-font-size", `${yesFontSize}rem`);

    if (noMessage) {
      noMessage.textContent = messages[Math.min(noClicks - 1, messages.length - 1)];
    }

    if (noClicks >= Math.max(1, clickLimit)) {
      convertNoButtonToYes();
      return;
    }

    moveNoButtonToRandomPosition();
  });
}

function convertNoButtonToYes() {
  noButtonConverted = true;
  noButton.textContent = yesButton.textContent.trim() || "YES";
  noButton.classList.remove("secondary-button");
  noButton.classList.add("primary-button");
  noButton.style.opacity = "";
  noButton.style.transform = "none";
  noButton.removeAttribute("aria-disabled");
  noButton.setAttribute("aria-label", "Yes");
}

function moveNoButtonToRandomPosition() {
  const buttonRect = noButton.getBoundingClientRect();
  const margin = 10;
  const maxLeft = window.innerWidth - buttonRect.width - margin;
  const maxTop = window.innerHeight - buttonRect.height - margin;

  if (maxLeft <= margin || maxTop <= margin) {
    return;
  }

  for (let attempt = 0; attempt < 90; attempt += 1) {
    const candidate = createRect(
      randomBetween(margin, maxLeft),
      randomBetween(margin, maxTop),
      buttonRect.width,
      buttonRect.height
    );

    if (isValidNoButtonPosition(candidate)) {
      applyNoButtonPosition(candidate);
      return;
    }
  }

  const fallback = findFallbackNoButtonPosition(buttonRect, margin);

  if (fallback) {
    applyNoButtonPosition(fallback);
  }
}

function applyNoButtonPosition(rect) {
  noButton.style.position = "fixed";
  noButton.style.left = `${Math.round(rect.left)}px`;
  noButton.style.top = `${Math.round(rect.top)}px`;
  noButton.style.right = "auto";
  noButton.style.bottom = "auto";
  noButton.style.zIndex = "30";
  noButton.style.transform = "none";
}

function isValidNoButtonPosition(candidate) {
  if (!isInsideViewport(candidate) || overlapsInvitationCard(candidate)) {
    return false;
  }

  return getCollisionRects().every((rect) => !rectanglesOverlap(candidate, rect));
}

function isInsideViewport(rect) {
  return rect.left >= 0
    && rect.top >= 0
    && rect.right <= window.innerWidth
    && rect.bottom <= window.innerHeight;
}

function overlapsInvitationCard(candidate) {
  if (!invitationCard) {
    return false;
  }

  return rectanglesOverlap(candidate, invitationCard.getBoundingClientRect());
}

function getCollisionRects() {
  const selectors = [
    ".top-bar",
    ".footer-note",
    ".invitation-card",
    ".invitation-card h1",
    ".invitation-card p",
    ".invitation-card a",
    ".invitation-card button",
    ".music-widget"
  ];

  return selectors
    .flatMap((selector) => Array.from(document.querySelectorAll(selector)))
    .filter((element) => element !== noButton && !noButton.contains(element))
    .map((element) => element.getBoundingClientRect())
    .filter((rect) => rect.width > 0 && rect.height > 0);
}

function findFallbackNoButtonPosition(buttonRect, margin) {
  const step = 24;

  for (let top = margin; top <= window.innerHeight - buttonRect.height - margin; top += step) {
    for (let left = margin; left <= window.innerWidth - buttonRect.width - margin; left += step) {
      const candidate = createRect(left, top, buttonRect.width, buttonRect.height);

      if (isValidNoButtonPosition(candidate)) {
        return candidate;
      }
    }
  }

  return null;
}

function rectanglesOverlap(first, second) {
  return first.left < second.right
    && first.right > second.left
    && first.top < second.bottom
    && first.bottom > second.top;
}

function createRect(left, top, width, height) {
  return {
    left,
    top,
    width,
    height,
    right: left + width,
    bottom: top + height
  };
}

function randomBetween(min, max) {
  return min + Math.random() * (max - min);
}
