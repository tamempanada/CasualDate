(() => {
  const root = document.querySelector("[data-love-blocks-root]");
  if (!root) return;

  const board = root.querySelector("[data-lb-board]");
  const piecesNode = root.querySelector("[data-lb-pieces]");
  const status = root.querySelector("[data-lb-status]");
  const dialog = root.querySelector("[data-lb-result]");
  const csrf = root.querySelector('input[name="__RequestVerificationToken"]')?.value || "";
  let state = null;
  let selectedPiece = -1;
  let busy = false;

  const post = async (url, body) => {
    const response = await fetch(url, {
      method: "POST",
      headers: { "Content-Type": "application/json", RequestVerificationToken: csrf },
      body: JSON.stringify(body || {}),
    });
    const data = await response.json().catch(() => ({}));
    if (!response.ok) throw new Error(data.message || (response.status === 401 ? "Log in as a player to save your best score." : "Love Blocks could not save that move."));
    return data.game;
  };

  const setStatus = (text, error = false) => {
    status.textContent = text;
    status.classList.toggle("is-error", error);
  };

  const render = () => {
    if (!state) return;
    root.querySelector("[data-lb-score]").textContent = state.score;
    root.querySelector("[data-lb-best]").textContent = state.bestScore;
    root.querySelector("[data-lb-lines]").textContent = state.linesCleared;
    root.querySelector("[data-lb-moves]").textContent = state.movesMade;
    root.querySelector("[data-lb-earned]").textContent = state.runEarned;
    root.querySelector("[data-lb-balance]").textContent = state.tokenBalance;
    root.querySelector("[data-lb-score-progress]").textContent = state.scoreRewardEnabled ? `${state.scoreProgress} pts to next (+${state.scoreRewardAmount})` : "Disabled";
    root.querySelector("[data-lb-line-progress]").textContent = state.lineRewardEnabled ? `${state.lineProgress} lines to next (+${state.lineRewardAmount})` : "Disabled";

    board.replaceChildren();
    for (let i = 0; i < 64; i++) {
      const cell = document.createElement("button");
      cell.type = "button";
      cell.className = `love-blocks-cell${state.board[i] === "#" ? " is-filled" : ""}`;
      if (state.clearedCells?.includes(i)) cell.classList.add("is-cleared");
      if (state.placedCells?.includes(i)) cell.classList.add("is-new");
      cell.dataset.row = String(Math.floor(i / 8));
      cell.dataset.column = String(i % 8);
      cell.setAttribute("role", "gridcell");
      cell.setAttribute("aria-label", `Row ${Math.floor(i / 8) + 1}, column ${i % 8 + 1}${state.board[i] === "#" ? ", filled" : ", empty"}`);
      board.append(cell);
    }

    piecesNode.replaceChildren();
    state.pieces.forEach((piece, index) => {
      const button = document.createElement("button");
      button.type = "button";
      button.className = `love-blocks-piece${selectedPiece === index ? " is-selected" : ""}`;
      button.dataset.pieceIndex = String(index);
      button.draggable = !state.isOver;
      button.setAttribute("aria-label", `Select piece ${index + 1}`);
      const rows = Math.max(...piece.cells.map(cell => cell.r)) + 1;
      const columns = Math.max(...piece.cells.map(cell => cell.c)) + 1;
      button.style.setProperty("--piece-rows", rows);
      button.style.setProperty("--piece-columns", columns);
      piece.cells.forEach(cell => {
        const block = document.createElement("span");
        block.className = "love-blocks-mini-cell";
        block.style.gridRow = String(cell.r + 1);
        block.style.gridColumn = String(cell.c + 1);
        button.append(block);
      });
      piecesNode.append(button);
    });

    if (state.isOver) {
      root.querySelector("[data-lb-final-score]").textContent = state.score;
      root.querySelector("[data-lb-final-best]").textContent = state.bestScore;
      root.querySelector("[data-lb-final-lines]").textContent = state.linesCleared;
      root.querySelector("[data-lb-final-moves]").textContent = state.movesMade;
      root.querySelector("[data-lb-final-earned]").textContent = state.runEarned;
      if (!dialog.open) dialog.showModal();
    }

    if (state.lastMovePoints > 0) {
      setStatus(`+${state.lastMovePoints} points${state.lastMoveLines ? ` · ${state.lastMoveLines} line${state.lastMoveLines === 1 ? "" : "s"} cleared` : ""}${state.lastMoveEarned ? ` · +${state.lastMoveEarned} tokens awarded` : ""}`);
    } else if (state.isOver) {
      setStatus("No remaining piece fits. Lovely run.");
    } else {
      setStatus(selectedPiece >= 0 ? "Piece selected. Choose a clear spot on the board." : "Choose a piece to begin.");
    }
  };

  const canPreview = (pieceIndex, row, column) => {
    if (!state?.pieces[pieceIndex]) return false;
    return state.pieces[pieceIndex].cells.every(cell => {
      const r = row + cell.r;
      const c = column + cell.c;
      return r >= 0 && r < 8 && c >= 0 && c < 8 && state.board[r * 8 + c] === ".";
    });
  };

  const clearPreview = () => board.querySelectorAll(".is-preview, .is-invalid").forEach(cell => cell.classList.remove("is-preview", "is-invalid"));
  const preview = (pieceIndex, row, column) => {
    clearPreview();
    const legal = canPreview(pieceIndex, row, column);
    if (!state?.pieces[pieceIndex]) return false;
    state.pieces[pieceIndex].cells.forEach(cell => {
      const target = board.querySelector(`[data-row="${row + cell.r}"][data-column="${column + cell.c}"]`);
      if (target) target.classList.add(legal ? "is-preview" : "is-invalid");
    });
    return legal;
  };

  const place = async (pieceIndex, row, column) => {
    if (busy || !state) return;
    if (!canPreview(pieceIndex, row, column)) {
      setStatus("That spot is occupied or out of bounds. Your piece is still available.", true);
      return;
    }
    busy = true;
    clearPreview();
    setStatus("Placing your piece...");
    try {
      state = await post("/love-blocks/place", {
        runToken: state.runToken,
        pieceIndex,
        row,
        column,
        requestId: crypto.randomUUID(),
      });
      selectedPiece = -1;
      render();
    } catch (error) {
      setStatus(error.message, true);
    } finally {
      busy = false;
    }
  };

  piecesNode.addEventListener("click", event => {
    const piece = event.target.closest("[data-piece-index]");
    if (!piece || busy) return;
    selectedPiece = Number(piece.dataset.pieceIndex);
    render();
  });
  piecesNode.addEventListener("dragstart", event => {
    const piece = event.target.closest("[data-piece-index]");
    if (!piece) return;
    selectedPiece = Number(piece.dataset.pieceIndex);
    event.dataTransfer.setData("text/plain", String(selectedPiece));
    event.dataTransfer.effectAllowed = "move";
  });
  board.addEventListener("dragover", event => {
    event.preventDefault();
    const cell = event.target.closest("[data-row]");
    if (cell) preview(selectedPiece, Number(cell.dataset.row), Number(cell.dataset.column));
  });
  board.addEventListener("dragleave", event => {
    if (!board.contains(event.relatedTarget)) clearPreview();
  });
  board.addEventListener("drop", event => {
    event.preventDefault();
    const cell = event.target.closest("[data-row]");
    const index = Number(event.dataTransfer.getData("text/plain"));
    if (cell) place(index, Number(cell.dataset.row), Number(cell.dataset.column));
  });
  board.addEventListener("pointerover", event => {
    const cell = event.target.closest("[data-row]");
    if (cell && selectedPiece >= 0) preview(selectedPiece, Number(cell.dataset.row), Number(cell.dataset.column));
  });
  board.addEventListener("pointerleave", clearPreview);
  board.addEventListener("click", event => {
    const cell = event.target.closest("[data-row]");
    if (cell && selectedPiece >= 0) place(selectedPiece, Number(cell.dataset.row), Number(cell.dataset.column));
  });

  const start = async () => {
    if (busy) return;
    busy = true;
    if (dialog.open) dialog.close();
    setStatus("Preparing a fresh board...");
    try {
      state = await post("/love-blocks/start");
      selectedPiece = -1;
      render();
    } catch (error) {
      setStatus(error.message, true);
    } finally {
      busy = false;
    }
  };
  root.querySelector("[data-lb-new-run]").addEventListener("click", start);
  root.querySelector("[data-lb-play-again]").addEventListener("click", start);

  fetch("/love-blocks/state", { headers: { Accept: "application/json" } })
    .then(async response => {
      const data = await response.json();
      if (!response.ok) throw new Error(data.message || "Could not load Love Blocks.");
      state = data.game;
      render();
    })
    .catch(error => setStatus(error.message, true));
})();
