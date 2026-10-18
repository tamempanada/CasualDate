const dropZone = document.querySelector(".drop-zone");
const fileInput = document.querySelector("#mediaFile");
const mediaFilterButtons = Array.from(document.querySelectorAll("[data-media-filter]"));
const mediaCards = Array.from(document.querySelectorAll("[data-media-card]"));
const mediaSearchInput = document.querySelector("#mediaSearchInput");
const mediaEmptyState = document.querySelector("[data-media-empty]");
const categoryFilter = document.querySelector("[data-media-category-filter]");
const gameFilter = document.querySelector("[data-media-game-filter]");
const sortControl = document.querySelector("[data-media-sort]");
const pagination = document.querySelector("[data-media-pagination]");
const pageSize = 18;
let currentPage = 1;

if (dropZone && fileInput) {
  ["dragenter", "dragover"].forEach((eventName) => {
    dropZone.addEventListener(eventName, (event) => {
      event.preventDefault();
      dropZone.classList.add("is-dragging");
    });
  });

  ["dragleave", "drop"].forEach((eventName) => {
    dropZone.addEventListener(eventName, (event) => {
      event.preventDefault();
      dropZone.classList.remove("is-dragging");
    });
  });

  dropZone.addEventListener("drop", (event) => {
    const [file] = event.dataTransfer.files;

    if (!file) {
      return;
    }

    const transfer = new DataTransfer();
    transfer.items.add(file);
    fileInput.files = transfer.files;
  });
}

function filterMediaCards() {
  const activeFilter = document.querySelector("[data-media-filter].active")?.dataset.mediaFilter || "all";
  const query = (mediaSearchInput?.value || "").trim().toLowerCase();
  const category = (categoryFilter?.value || "").toLowerCase();
  const game = gameFilter?.value || "";
  const matches = mediaCards.filter((card) => {
    const typeMatches = activeFilter === "all" || card.dataset.mediaType === activeFilter;
    const searchMatches = !query || (card.dataset.mediaSearch || "").includes(query);
    const categoryMatches = !category || (card.dataset.mediaCategory || "").toLowerCase() === category;
    const gameMatches = !game || card.dataset.mediaGame === game;
    return typeMatches && searchMatches && categoryMatches && gameMatches;
  });

  const sort = sortControl?.value || "newest";
  matches.sort((left, right) => {
    if (sort === "name") return (left.dataset.mediaName || "").localeCompare(right.dataset.mediaName || "");
    const comparison = Number(right.dataset.mediaCreated || 0) - Number(left.dataset.mediaCreated || 0);
    return sort === "oldest" ? -comparison : comparison;
  });
  matches.forEach((card) => mediaCards[0]?.parentElement?.append(card));

  const pageCount = Math.max(1, Math.ceil(matches.length / pageSize));
  currentPage = Math.min(currentPage, pageCount);
  const start = (currentPage - 1) * pageSize;

  mediaCards.forEach((card) => { card.hidden = true; });
  matches.slice(start, start + pageSize).forEach((card) => { card.hidden = false; });

  if (mediaEmptyState) {
    mediaEmptyState.hidden = matches.length > 0;
  }

  if (pagination) {
    pagination.innerHTML = pageCount > 1
      ? Array.from({ length: pageCount }, (_, index) => `<button type="button" class="${index + 1 === currentPage ? "active" : ""}" data-media-page="${index + 1}">${index + 1}</button>`).join("")
      : "";
  }
}

mediaFilterButtons.forEach((button) => {
  button.addEventListener("click", () => {
    mediaFilterButtons.forEach((item) => item.classList.toggle("active", item === button));
    currentPage = 1;
    filterMediaCards();
  });
});

mediaSearchInput?.addEventListener("input", () => { currentPage = 1; filterMediaCards(); });
[categoryFilter, gameFilter, sortControl].forEach((control) => control?.addEventListener("change", () => { currentPage = 1; filterMediaCards(); }));
pagination?.addEventListener("click", (event) => {
  const page = Number(event.target.closest("[data-media-page]")?.dataset.mediaPage);
  if (page) { currentPage = page; filterMediaCards(); pagination.scrollIntoView({ block: "nearest" }); }
});
filterMediaCards();
