window.CasualDateGameRewards = (() => {
  const startAttempts = new Map();

  const request = async (url, body) => {
    const response = await fetch(url, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(body)
    });

    if (response.status === 401) {
      window.location.href = `/player/login?returnUrl=${encodeURIComponent(window.location.pathname)}`;
      return null;
    }

    const data = await response.json().catch(() => ({}));
    if (!response.ok) {
      throw new Error(data.message || "This game could not be completed.");
    }

    return data;
  };

  const startAttempt = async (gameSlug, forceNew = false) => {
    if (forceNew) {
      startAttempts.delete(gameSlug);
    }

    if (!startAttempts.has(gameSlug)) {
      startAttempts.set(gameSlug, request("/game-rewards/start", { gameSlug }));
    }

    try {
      const result = await startAttempts.get(gameSlug);
      return result?.attemptToken || null;
    } catch (error) {
      startAttempts.delete(gameSlug);
      throw error;
    }
  };

  const complete = async (gameSlug, selection) => {
    const attemptToken = await startAttempt(gameSlug);
    if (!attemptToken) {
      return null;
    }

    const result = await request(`/game-rewards/${gameSlug}/complete`, { attemptToken, ...selection });
    if (result?.succeeded) {
      startAttempts.delete(gameSlug);
    }
    return result;
  };

  const registerPhotoboothCapture = async () => {
    const attemptToken = await startAttempt("photobooth");
    if (!attemptToken) {
      return null;
    }

    return request("/game-rewards/photobooth/capture", { attemptToken });
  };

  const completePhotobooth = async () => {
    const attemptToken = await startAttempt("photobooth");
    if (!attemptToken) {
      return null;
    }

    const result = await request("/game-rewards/photobooth/complete", { attemptToken });
    if (result?.succeeded) {
      startAttempts.delete("photobooth");
    }
    return result;
  };

  return { startAttempt, complete, registerPhotoboothCapture, completePhotobooth };
})();
