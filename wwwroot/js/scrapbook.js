const scrapbookRoot = document.querySelector("[data-scrapbook-root]");

if (scrapbookRoot) {
  const tabsNode = scrapbookRoot.querySelector("[data-scrapbook-tabs]");
  const prevButton = scrapbookRoot.querySelector("[data-scrapbook-prev]");
  const nextButton = scrapbookRoot.querySelector("[data-scrapbook-next]");
  const statusNode = scrapbookRoot.querySelector("[data-scrapbook-status]");
  const paperNode = scrapbookRoot.querySelector("[data-scrapbook-paper]");
  const titleInput = scrapbookRoot.querySelector("[data-scrapbook-title]");
  const dateInput = scrapbookRoot.querySelector("[data-scrapbook-date]");
  const bodyInput = scrapbookRoot.querySelector("[data-scrapbook-body]");
  const designInput = scrapbookRoot.querySelector("[data-scrapbook-design]");
  const fontSizeInput = scrapbookRoot.querySelector("[data-scrapbook-font-size]");
  const colorInput = scrapbookRoot.querySelector("[data-scrapbook-color]");
  const alignInput = scrapbookRoot.querySelector("[data-scrapbook-align]");
  const editableControls = [titleInput, dateInput, bodyInput, designInput, fontSizeInput, colorInput, alignInput];
  let pages = [];
  let currentIndex = 0;
  let saveTimer = null;
  let isLoadingPage = false;

  const requestJson = async (url, options = {}) => {
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
      throw new Error(data.message || "Scrapbook is unavailable.");
    }

    return data;
  };

  const setStatus = (message, kind = "") => {
    statusNode.textContent = message;
    statusNode.dataset.status = kind;
  };

  const currentPage = () => pages[currentIndex];

  const applyPaperStyle = (page) => {
    paperNode.className = `scrapbook-paper is-${page.pageDesign || "lined"}`;
    bodyInput.style.fontSize = `${page.fontSize || 18}px`;
    bodyInput.style.color = page.textColor || "#3a2724";
    bodyInput.style.textAlign = page.textAlign || "left";
  };

  const renderTabs = () => {
    tabsNode.innerHTML = pages
      .map((page, index) => `
        <button type="button" class="${index === currentIndex ? "is-active" : ""}" data-page-index="${index}">
          <span>${page.pageNumber}</span>
          <strong>${page.title || `Page ${page.pageNumber}`}</strong>
        </button>
      `)
      .join("");

    tabsNode.querySelectorAll("[data-page-index]").forEach((button) => {
      button.addEventListener("click", () => {
        showPage(Number(button.dataset.pageIndex || 0));
      });
    });
  };

  const showPage = (index) => {
    if (!pages[index]) {
      return;
    }

    isLoadingPage = true;
    currentIndex = index;
    const page = currentPage();
    titleInput.value = page.title || "";
    dateInput.value = page.dateLabel || "";
    bodyInput.value = page.body || "";
    designInput.value = page.pageDesign || "lined";
    fontSizeInput.value = page.fontSize || 18;
    colorInput.value = page.textColor || "#3a2724";
    alignInput.value = page.textAlign || "left";
    applyPaperStyle(page);
    renderTabs();
    setStatus("Saved", "saved");
    isLoadingPage = false;
  };

  const readCurrentForm = () => ({
    pageNumber: currentPage().pageNumber,
    title: titleInput.value,
    dateLabel: dateInput.value,
    body: bodyInput.value,
    pageDesign: designInput.value,
    textColor: colorInput.value,
    textAlign: alignInput.value,
    fontSize: Number(fontSizeInput.value || 18),
  });

  const saveCurrentPage = async () => {
    if (!currentPage()) {
      return;
    }

    setStatus("Saving...", "saving");

    try {
      const data = await requestJson("/scrapbook/save", {
        method: "POST",
        body: JSON.stringify(readCurrentForm()),
      });

      if (!data) {
        return;
      }

      pages[currentIndex] = data.page;
      applyPaperStyle(data.page);
      renderTabs();
      setStatus("Saved", "saved");
    } catch (error) {
      setStatus("Save failed", "error");
    }
  };

  const queueSave = () => {
    if (isLoadingPage) {
      return;
    }

    const draft = readCurrentForm();
    pages[currentIndex] = {
      ...currentPage(),
      ...draft,
      dateLabel: draft.dateLabel,
    };
    applyPaperStyle(pages[currentIndex]);
    renderTabs();
    setStatus("Saving...", "saving");
    window.clearTimeout(saveTimer);
    saveTimer = window.setTimeout(saveCurrentPage, 650);
  };

  editableControls.forEach((control) => {
    control.addEventListener("input", queueSave);
    control.addEventListener("change", queueSave);
  });

  prevButton.addEventListener("click", () => showPage(Math.max(0, currentIndex - 1)));
  nextButton.addEventListener("click", () => showPage(Math.min(pages.length - 1, currentIndex + 1)));

  const loadPages = async () => {
    setStatus("Loading...", "saving");

    try {
      const data = await requestJson("/scrapbook/pages");

      if (!data) {
        return;
      }

      pages = data.pages || [];

      if (pages.length === 0) {
        tabsNode.innerHTML = `<p class="soft-copy">No pages yet.</p>`;
        setStatus("Empty", "error");
        return;
      }

      showPage(0);
    } catch (error) {
      tabsNode.innerHTML = `<p class="soft-copy">${error.message}</p>`;
      setStatus("Unavailable", "error");
    }
  };

  loadPages();
}
