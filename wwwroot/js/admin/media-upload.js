const dropZone = document.querySelector(".drop-zone");
const fileInput = document.querySelector("#mediaFile");
const mediaFilterButtons = Array.from(document.querySelectorAll("[data-media-filter]"));
const mediaCards = Array.from(document.querySelectorAll("[data-media-card]"));
const mediaSearchInput = document.querySelector("#mediaSearchInput");
const mediaEmptyState = document.querySelector("[data-media-empty]");

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
  let visibleCount = 0;

  mediaCards.forEach((card) => {
    const typeMatches = activeFilter === "all" || card.dataset.mediaType === activeFilter;
    const searchMatches = !query || (card.dataset.mediaSearch || "").includes(query);
    const isVisible = typeMatches && searchMatches;

    card.hidden = !isVisible;

    if (isVisible) {
      visibleCount += 1;
    }
  });

  if (mediaEmptyState) {
    mediaEmptyState.hidden = visibleCount > 0;
  }
}

mediaFilterButtons.forEach((button) => {
  button.addEventListener("click", () => {
    mediaFilterButtons.forEach((item) => item.classList.toggle("active", item === button));
    filterMediaCards();
  });
});

mediaSearchInput?.addEventListener("input", filterMediaCards);
filterMediaCards();
