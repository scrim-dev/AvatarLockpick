/**
 * Button Action Handlers for AvatarLockpick GUI
 */

// --- Photino Communication Wrapper ---
function sendMessageToDotNet(messageObject) {
    if (window.external && typeof window.external.sendMessage === 'function') {
        const messageString = JSON.stringify(messageObject);
        console.log("Sending to .NET:", messageString);
        window.external.sendMessage(messageString);
    } else {
        console.error("Photino communication (window.external.sendMessage) not available.");
    }
}

// --- Button Click Functions ---

/**
 * Sends Avatar ID and User ID to the .NET backend.
 * Assumes elements with IDs 'avatar-id' and 'user-id' exist.
 */
function callDotNetSendIDs() {
    // It's better practice to get elements inside the function if they might not exist yet
    // or if this script loads before the main script defines the cached elements.
    // However, for simplicity, we'll assume they are globally available from the inline script for now.
    const avatarIdInput = document.getElementById('avatar-id');
    const userIdInputElement = document.getElementById('user-id'); // Renamed to avoid conflict with inline script var

    if (!avatarIdInput || !userIdInputElement) {
        console.error("Required input elements not found.");
        return;
    }

    const avatarId = avatarIdInput.value;
    const userId = userIdInputElement.value; // Get current value regardless of censoring

    const message = { type: 'avatarInfo', avatarId, userId };
    sendMessageToDotNet(message);
}

/**
 * Simulates loading avatar info based on entered IDs.
 * Updates the UI in the Unlock tab.
 */
function loadAvatarInfo() {
    const avatarIdInput = document.getElementById('avatar-id');
    const userIdInputElement = document.getElementById('user-id');

    if (!avatarIdInput || !userIdInputElement) {
        console.error("Required input elements not found.");
        // Maybe show an error popup?
        return;
    }

    const avatarId = avatarIdInput.value;
    const userId = userIdInputElement.value;

    if (!avatarId) { // Basic validation
        console.error("Avatar ID is required.");
        // Show error popup
        alert("Please enter an Avatar ID.");
        return;
    }

    console.log(`Loading info for Avatar: ${avatarId}, User: ${userId || 'N/A'}`);

    // --- Update the global state and UI (assuming vars/functions exist) ---
    if (typeof loadedAvatarIdSpan !== 'undefined' && typeof loadedUserIdSpan !== 'undefined') {
        loadedAvatarIdSpan.textContent = avatarId;
        loadedUserIdSpan.textContent = censorUserIdToggle.checked ? '********' : (userId || 'N/A');
    } else {
        console.error("Loaded info spans not found.");
    }

    if (typeof isAvatarLoaded !== 'undefined' && typeof loadedAvatarId !== 'undefined' && typeof loadedUserId !== 'undefined') {
        isAvatarLoaded = true; // Set the global flag
        // Store the IDs globally (assuming loadedAvatarId and loadedUserId are declared in inline script)
        window.loadedAvatarId = avatarId; // Assign to window or use a dedicated object if preferred
        window.loadedUserId = userId;
    } else {
         console.error("Global state variables (isAvatarLoaded, loadedAvatarId, loadedUserId) not found.");
    }

    if (typeof updateUnlockUI === 'function') {
        updateUnlockUI(); // Call the UI update function
    } else {
        console.error("updateUnlockUI function not found.");
    }

    // Optionally, send a message to .NET to confirm loading
    // sendMessageToDotNet({ type: 'avatarLoaded', avatarId });

    // Optionally, switch to the Unlock tab
    // document.querySelector('.tab-button[data-tab="unlock"]').click();
}

/**
 * Triggers the display of the action popup.
 * Assumes elements with IDs 'action-popup', 'popup-title', 'popup-message' exist.
 * @param {string} actionType - The type of action being triggered (e.g., 'Unlock', 'Unlock All').
 */
function triggerPopup(actionType) {
    // Check if avatar is loaded before proceeding
    if (!isAvatarLoaded || !window.loadedAvatarId) {
        console.error("No avatar loaded. Cannot perform unlock action.");
        alert("Please load an avatar first.");
        return;
    }

    console.log(`${actionType} button clicked for Avatar: ${window.loadedAvatarId}`);

    // Again, assuming global elements for simplicity
    const actionPopupElement = document.getElementById('action-popup');
    const popupTitleElement = document.getElementById('popup-title');
    const popupMessageElement = document.getElementById('popup-message');

    if (!actionPopupElement || !popupTitleElement || !popupMessageElement) {
         console.error("Popup elements not found.");
        return;
    }

    popupTitleElement.textContent = `${actionType} Action`;
    popupMessageElement.textContent = `Calling .NET...`;
    actionPopupElement.classList.add('show');

    // Example: Send message to .NET including loaded avatar details
    const message = {
        type: 'unlockAction',
        action: actionType,
        avatarId: window.loadedAvatarId, // Include loaded Avatar ID
        userId: window.loadedUserId || null // Include loaded User ID (or null if none)
    };
    sendMessageToDotNet(message);
}

/**
 * Closes the action popup.
 * Assumes element with ID 'action-popup' exists.
 */
function closePopup() {
     // Assuming global element
    const actionPopupElement = document.getElementById('action-popup');
     if (!actionPopupElement) {
         console.error("Popup element not found.");
        return;
    }
    actionPopupElement.classList.remove('show');
}

/**
 * Sends a request to the .NET backend to open the help/documentation URL.
 */
function openHelpUrl() {
    console.log("Requesting help URL from .NET...");
    const message = { type: 'openHelpUrl' };
    sendMessageToDotNet(message);
} 