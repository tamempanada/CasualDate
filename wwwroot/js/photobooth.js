const page = document.querySelector(".photobooth-page");
const video = document.querySelector("#cameraPreview");
const captureCanvas = document.querySelector("#captureCanvas");
const stripCanvas = document.querySelector("#stripCanvas");
const countdownOverlay = document.querySelector("#countdownOverlay");
const flashOverlay = document.querySelector("#flashOverlay");
const cameraFallback = document.querySelector("#cameraFallback");
const startCameraButton = document.querySelector("#startCameraButton");
const takePhotoButton = document.querySelector("#takePhotoButton");
const useFallbackButton = document.querySelector("#useFallbackButton");
const downloadStripButton = document.querySelector("#downloadStripButton");
const retakeStripButton = document.querySelector("#retakeStripButton");
const completePhotoboothButton = document.querySelector("#completePhotoboothButton");
const finishPhotoboothButton = document.querySelector("#finishPhotoboothButton");
const photoboothCompletion = document.querySelector("#photoboothCompletion");
const photoboothCompletionImage = document.querySelector("#photoboothCompletionImage");
const photoboothCompletionHeading = document.querySelector("#photoboothCompletionHeading");
const photoboothCompletionSubheading = document.querySelector("#photoboothCompletionSubheading");
const photoSlots = document.querySelector("#photoSlots");
const statusText = document.querySelector("#photoboothStatus");

const photoCount = Number(page?.dataset.photoCount || 4);
const stripTemplateSrc = "/images/photobooth/photostrip-template.png";
const stripOverlaySrc = "/images/photobooth/photostrip-template-overlay.png";
const stripFrames = [
  { x: 52, y: 249, width: 504, height: 290, radius: 38 },
  { x: 52, y: 573, width: 504, height: 295, radius: 38 },
  { x: 52, y: 898, width: 504, height: 296, radius: 38 },
  { x: 52, y: 1225, width: 504, height: 295, radius: 38 }
];
const photos = [];
let stream = null;
let isCapturing = false;
let stripRenderId = 0;
let serverCapturedPhotoCount = 0;

function loadImage(source) {
  return new Promise((resolve, reject) => {
    const image = new Image();
    image.onload = () => resolve(image);
    image.onerror = reject;
    image.src = source;
  });
}

const stripTemplateReady = Promise.all([
  loadImage(stripTemplateSrc),
  loadImage(stripOverlaySrc)
]);

function setStatus(message) {
  if (statusText) {
    statusText.textContent = message;
  }
}

function createSlots() {
  if (!photoSlots) {
    return;
  }

  photoSlots.innerHTML = "";

  for (let index = 0; index < photoCount; index += 1) {
    const slot = document.createElement("div");
    slot.className = "photo-slot";
    slot.textContent = String(index + 1);
    photoSlots.appendChild(slot);
  }
}

function updateSlots() {
  document.querySelectorAll(".photo-slot").forEach((slot, index) => {
    slot.innerHTML = "";

    if (photos[index]) {
      const image = document.createElement("img");
      image.src = photos[index];
      image.alt = `Photobooth capture ${index + 1}`;
      slot.appendChild(image);
      slot.classList.add("filled");
    } else {
      slot.textContent = String(index + 1);
      slot.classList.remove("filled");
    }
  });
}

async function startCamera() {
  if (!navigator.mediaDevices?.getUserMedia) {
    showFallback("Camera is not supported in this browser.");
    return;
  }

  try {
    stream = await navigator.mediaDevices.getUserMedia({
      video: {
        facingMode: "user",
        width: { ideal: 1280 },
        height: { ideal: 720 }
      },
      audio: false
    });

    video.srcObject = stream;
    video.hidden = false;
    cameraFallback.hidden = true;
    takePhotoButton.disabled = false;
    setStatus("Camera ready. Take the first photo when you are ready.");

    try {
      await window.CasualDateGameRewards?.startAttempt("photobooth", true);
    } catch {
      // Camera capture must remain available even when optional token verification is unavailable.
      setStatus("Camera ready. Take photos normally; token verification is temporarily unavailable.");
    }
  } catch {
    showFallback("Camera permission was denied or unavailable.");
  }
}

function showFallback(message) {
  if (video) {
    video.hidden = true;
  }

  if (cameraFallback) {
    cameraFallback.hidden = false;
  }

  if (takePhotoButton) {
    takePhotoButton.disabled = true;
  }

  setStatus(`${message} Placeholder mode is available.`);
}

function wait(milliseconds) {
  return new Promise((resolve) => window.setTimeout(resolve, milliseconds));
}

async function runCountdown() {
  for (const value of ["3", "2", "1"]) {
    countdownOverlay.textContent = value;
    await wait(700);
  }

  countdownOverlay.textContent = "FLASH!";
  flashOverlay.classList.add("active");
  await wait(160);
  flashOverlay.classList.remove("active");
  countdownOverlay.textContent = "";
}

async function takePhoto() {
  if (isCapturing || photos.length >= photoCount) {
    return;
  }

  isCapturing = true;
  takePhotoButton.disabled = true;

  await runCountdown();

  photos.push(captureFromVideo());
  try {
    const capture = await window.CasualDateGameRewards?.registerPhotoboothCapture();
    if (capture?.succeeded) {
      serverCapturedPhotoCount = capture.captureCount;
    }
  } catch {
    setStatus("Photo captured, but its token eligibility could not be verified. You can still save your strip.");
  }
  updateSlots();
  await drawStrip();

  if (photos.length >= photoCount) {
    downloadStripButton.disabled = false;
    completePhotoboothButton.disabled = false;
    setStatus("Photo strip ready. Save it or retake if you want another try.");
  } else {
    takePhotoButton.disabled = false;
    setStatus(`${photos.length} / ${photoCount} photos captured.`);
  }

  isCapturing = false;
}

function captureFromVideo() {
  const width = video.videoWidth || 900;
  const height = video.videoHeight || 700;
  captureCanvas.width = width;
  captureCanvas.height = height;

  const context = captureCanvas.getContext("2d");
  context.save();
  context.translate(width, 0);
  context.scale(-1, 1);
  context.drawImage(video, 0, 0, width, height);
  context.restore();

  return captureCanvas.toDataURL("image/jpeg", 0.92);
}

function makePlaceholderPhoto(index) {
  const canvas = document.createElement("canvas");
  canvas.width = 900;
  canvas.height = 700;
  const context = canvas.getContext("2d");
  const hue = 345 + index * 12;

  context.fillStyle = `hsl(${hue}, 80%, 94%)`;
  context.fillRect(0, 0, canvas.width, canvas.height);
  context.fillStyle = "rgba(216, 79, 111, 0.18)";

  for (let i = 0; i < 18; i += 1) {
    context.beginPath();
    context.arc(Math.random() * canvas.width, Math.random() * canvas.height, 18 + Math.random() * 45, 0, Math.PI * 2);
    context.fill();
  }

  context.fillStyle = "#332522";
  context.font = "700 54px Segoe UI, Arial";
  context.textAlign = "center";
  context.fillText("Casual Date", canvas.width / 2, canvas.height / 2 - 14);
  context.font = "500 28px Segoe UI, Arial";
  context.fillText(`Placeholder photo ${index + 1}`, canvas.width / 2, canvas.height / 2 + 38);

  return canvas.toDataURL("image/jpeg", 0.92);
}

async function fillWithPlaceholders() {
  while (photos.length < photoCount) {
    photos.push(makePlaceholderPhoto(photos.length));
  }

  updateSlots();
  await drawStrip();
  downloadStripButton.disabled = false;
  completePhotoboothButton.disabled = false;
  setStatus("Placeholder strip ready.");
}

async function drawStrip() {
  const renderId = (stripRenderId += 1);
  const context = stripCanvas.getContext("2d");
  const [templateImage, overlayImage] = await stripTemplateReady;

  if (renderId !== stripRenderId) {
    return;
  }

  stripCanvas.width = templateImage.width;
  stripCanvas.height = templateImage.height;
  context.clearRect(0, 0, stripCanvas.width, stripCanvas.height);
  context.drawImage(templateImage, 0, 0, stripCanvas.width, stripCanvas.height);

  const loadedPhotos = await Promise.all(photos.map((photo) => loadImage(photo)));

  if (renderId !== stripRenderId) {
    return;
  }

  loadedPhotos.slice(0, stripFrames.length).forEach((photo, index) => {
    const frame = stripFrames[index];
    context.save();
    roundedRectanglePath(context, frame.x, frame.y, frame.width, frame.height, frame.radius);
    context.clip();
    drawImageCover(context, photo, frame.x, frame.y, frame.width, frame.height);
    context.restore();
  });

  context.drawImage(overlayImage, 0, 0, stripCanvas.width, stripCanvas.height);
}

function drawImageCover(context, image, x, y, width, height) {
  const ratio = Math.max(width / image.width, height / image.height);
  const drawWidth = image.width * ratio;
  const drawHeight = image.height * ratio;
  const drawX = x + (width - drawWidth) / 2;
  const drawY = y + (height - drawHeight) / 2;
  context.drawImage(image, drawX, drawY, drawWidth, drawHeight);
}

function roundedRectanglePath(context, x, y, width, height, radius) {
  const safeRadius = Math.min(radius, width / 2, height / 2);
  context.beginPath();
  context.moveTo(x + safeRadius, y);
  context.lineTo(x + width - safeRadius, y);
  context.quadraticCurveTo(x + width, y, x + width, y + safeRadius);
  context.lineTo(x + width, y + height - safeRadius);
  context.quadraticCurveTo(x + width, y + height, x + width - safeRadius, y + height);
  context.lineTo(x + safeRadius, y + height);
  context.quadraticCurveTo(x, y + height, x, y + height - safeRadius);
  context.lineTo(x, y + safeRadius);
  context.quadraticCurveTo(x, y, x + safeRadius, y);
  context.closePath();
}

async function downloadStrip() {
  await drawStrip();
  const link = document.createElement("a");
  link.download = "casual-date-photobooth-strip.png";
  link.href = stripCanvas.toDataURL("image/png");
  link.click();
}

async function resetPhotobooth() {
  photos.splice(0, photos.length);
  updateSlots();
  await drawStrip();
  downloadStripButton.disabled = true;
  completePhotoboothButton.disabled = true;
  if (photoboothCompletion) {
    photoboothCompletion.hidden = true;
  }

  if (stream) {
    takePhotoButton.disabled = false;
    serverCapturedPhotoCount = 0;
    try {
      await window.CasualDateGameRewards?.startAttempt("photobooth", true);
    } catch {
      // The strip remains usable even if its optional token verification cannot start.
    }
  }

  setStatus(stream ? "Ready for a new strip." : "Ready for placeholder mode or camera access.");
}

async function showPhotoboothCompletion() {
  if (!photoboothCompletion || !photoboothCompletionImage) {
    return;
  }

  await drawStrip();
  photoboothCompletionImage.src = stripCanvas.toDataURL("image/png");

  if (photoboothCompletionHeading) {
    photoboothCompletionHeading.textContent = page?.dataset.completionHeading || "Game Completed!";
  }

  if (photoboothCompletionSubheading) {
    photoboothCompletionSubheading.textContent = page?.dataset.completionSubheading || "Your smile is as beautiful as always.";
  }

  photoboothCompletion.hidden = false;
  photoboothCompletion.scrollIntoView({ behavior: "smooth", block: "center" });
}

createSlots();
drawStrip();

startCameraButton?.addEventListener("click", startCamera);
takePhotoButton?.addEventListener("click", takePhoto);
useFallbackButton?.addEventListener("click", fillWithPlaceholders);
downloadStripButton?.addEventListener("click", downloadStrip);
retakeStripButton?.addEventListener("click", resetPhotobooth);
completePhotoboothButton?.addEventListener("click", showPhotoboothCompletion);
finishPhotoboothButton?.addEventListener("click", async () => {
  if (finishPhotoboothButton.disabled) {
    return;
  }

  finishPhotoboothButton.disabled = true;
  let reward = null;

  if (serverCapturedPhotoCount >= photoCount) {
    try {
      const completion = await window.CasualDateGameRewards?.completePhotobooth();
      reward = completion?.reward || null;
      if (reward?.awarded) {
        setStatus(`Photobooth complete. +${reward.amount} tokens awarded.`);
      }
    } catch (error) {
      setStatus(error.message || "Your strip is complete, but its token reward could not be verified.");
    }
  } else {
    setStatus("Photobooth complete. Placeholder photos do not earn tokens.");
  }

  await window.CasualDatePlayerProgress?.completeGame(finishPhotoboothButton, "photobooth", {
    skipReward: true,
    reward
  });
});

function stopPhotoboothStream() {
  stream?.getTracks().forEach((track) => track.stop());
}

window.addEventListener("pagehide", stopPhotoboothStream);
window.addEventListener("casual-date:before-page-swap", stopPhotoboothStream, { once: true });
