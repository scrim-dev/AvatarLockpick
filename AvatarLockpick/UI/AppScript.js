/* Full App Script */

// --- Elements Cache ---
let appWrapper, tabButtons, tabPanels, animationsToggle, userIdInput, censorUserIdToggle;
let avatarIdInput, responseSpan, colorPickers, htmlElement, actionPopup, popupTitle, popupMessage;
let unlockNoticePopup, noticeTimerSpan, sidebarToggle, loadedAvatarInfoPanel, loadedAvatarIdSpan;
let loadedUserIdSpan, unlockActionButtons, unlockWarningPanel, confirmUnloadPopup, presetButtons;
let historyListContainer, noHistoryMessage, importJsonPopup, importJsonTextarea, importErrorMessage;
let netResponsePopup, netResponseContent, notePopup, noteTextarea, appLogsContainer, backendMessagesToggle;
let apiStatusIcon, avatarInfoPopup, modalAvatarImg, modalAvatarName, modalAvatarAuthor, modalAvatarDesc;
let loadedAvatarImg, loadedAvatarName, loadedAvatarAuthor, currentModalAvatarId;
let _pendingUpdateVersion = null;
let _updateCheckInProgress = false;

// --- Theme Objects ---
const themes = {
    dodgerblue: {
        '--bg-color': '#121212',
        '--bg-secondary-color': '#181818',
        '--text-color': '#ffffff',
        '--accent-color': '#1E90FF',
        '--accent-hover-color': '#46A3FF',
        '--panel-bg-color': '#242424',
        '--border-color': '#333333',
        '--input-bg-color': '#2d2d2d',
        '--input-border-color': '#454545'
    },
    crimson: {
        '--accent-color': '#DC143C',
        '--accent-hover-color': '#E6395A',
    },
    green: {
        '--accent-color': '#2ECC71',
        '--accent-hover-color': '#58D68D',
    },
    magenta: {
        '--accent-color': '#DE3BFF',
        '--accent-hover-color': '#E566FF',
    }
};

const defaultTheme = themes.dodgerblue;

// Global state variables
let isAvatarLoaded = false;
let loadedAvatarId = null;
let loadedUserId = null;
let avatarHistory = [];
let netLog = ["Waiting for messages from .NET..."];
let currentEditingAvatarId = null;
let noticeShownThisSession = false;
let noticeTimerInterval = null;
let showBackendMessages = false;
let toastNotificationsEnabled = false;
let _unlockInProgress = false;

function _setUnlockButtonsDisabled(disabled) {
    _unlockInProgress = disabled;
    document.querySelectorAll('.unlock-action-btn').forEach(function(btn) {
        btn.disabled = disabled || !isAvatarLoaded;
    });
}

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

// --- Helper Functions ---
function applyTheme(themeSettings, saveToLocalStorage = true) {
    Object.keys(themeSettings).forEach(key => {
        if (themeSettings.hasOwnProperty(key)) {
            htmlElement.style.setProperty(key, themeSettings[key]);
        }
    });
    if (themeSettings.hasOwnProperty('--accent-color')) {
        updateAccentRgbVar(themeSettings['--accent-color']);
    }
    updateColorPickers();

    if (saveToLocalStorage) {
        let matchedPreset = null;
        for (const name in themes) {
            const presetFull = { ...defaultTheme, ...themes[name] };
            let isMatch = true;
            Object.keys(presetFull).forEach(key => {
                if (themeSettings[key] !== presetFull[key]) {
                    isMatch = false;
                }
            });
            if (isMatch && Object.keys(themeSettings).length === Object.keys(presetFull).length) {
                matchedPreset = name;
                break;
            }
        }

        if (matchedPreset) {
            console.log(`Saving applied theme as preset: ${matchedPreset}`);
            localStorage.setItem('themeType', 'preset');
            localStorage.setItem('themeName', matchedPreset);
            colorPickers.forEach(picker => {
                const cssVar = picker.getAttribute('data-var');
                localStorage.removeItem(`customColor_${cssVar}`);
            });
        } else {
            console.log("Saving applied theme as custom.");
            localStorage.setItem('themeType', 'custom');
            localStorage.removeItem('themeName');
            Object.keys(themeSettings).forEach(key => {
                if (key.startsWith('--')) {
                    localStorage.setItem(`customColor_${key}`, themeSettings[key]);
                }
            });
        }
    }
}

function updateColorPickers() {
    colorPickers.forEach(picker => {
        const cssVar = picker.getAttribute('data-var');
        picker.value = getComputedStyle(htmlElement).getPropertyValue(cssVar).trim();
    });
}

function hexToRgb(hex) {
    let result = /^#?([a-f\d]{2})([a-f\d]{2})([a-f\d]{2})$/i.exec(hex);
    return result ? {
        r: parseInt(result[1], 16),
        g: parseInt(result[2], 16),
        b: parseInt(result[3], 16)
    } : null;
}

function updateAccentRgbVar(hexColor) {
    const rgb = hexToRgb(hexColor);
    if (rgb) {
        htmlElement.style.setProperty('--accent-color-rgb', `${rgb.r},${rgb.g},${rgb.b}`);
    }
}

// --- Initialization ---
function initialize() {
    const savedThemeType = localStorage.getItem('themeType');
    const savedThemeName = localStorage.getItem('themeName');
    let themeToApply = { ...defaultTheme };

    if (savedThemeType === 'preset' && savedThemeName && themes[savedThemeName]) {
        console.log(`Initializing with saved preset theme: ${savedThemeName}`);
        themeToApply = { ...defaultTheme, ...themes[savedThemeName] };
    } else if (savedThemeType === 'custom') {
        console.log("Initializing with saved custom theme settings.");
        colorPickers.forEach(picker => {
            const cssVar = picker.getAttribute('data-var');
            const savedColor = localStorage.getItem(`customColor_${cssVar}`);
            if (savedColor) {
                themeToApply[cssVar] = savedColor;
            }
        });
    } else {
        console.log("Initializing with default theme (Dodger Blue).");
    }
    applyTheme(themeToApply, false);

    sendMessageToDotNet({ type: 'checkPawStatus' });
    sendMessageToDotNet({ type: 'checkForUpdates' });
    _updateExtrasUpdateUI('checking');

    const sidebarExpandedStr = localStorage.getItem('sidebarExpanded');
    const sidebarExpanded = sidebarExpandedStr !== 'false';
    appWrapper.classList.toggle('sidebar-expanded', sidebarExpanded);
    updateSidebarToggleIcon(sidebarExpanded);

    const savedAnimationPref = localStorage.getItem('animationsEnabled');
    if (savedAnimationPref !== null) {
        animationsToggle.checked = JSON.parse(savedAnimationPref);
    }
    document.body.classList.toggle('no-animations', !animationsToggle.checked);

    const savedCensorPref = localStorage.getItem('censorUserId');
    if (savedCensorPref !== null) {
        censorUserIdToggle.checked = JSON.parse(savedCensorPref);
        userIdInput.type = censorUserIdToggle.checked ? 'password' : 'text';
    }

    const noticeAlreadyShown = localStorage.getItem('unlockNoticeShown') === 'true';
    if (noticeAlreadyShown) {
        noticeShownThisSession = true;
        console.log("Unlock notice has been shown in a previous session.");
    }

    const savedAvatarId = localStorage.getItem('avatarId');
    const savedUserId = localStorage.getItem('userId');
    if (savedAvatarId) {
        avatarIdInput.value = savedAvatarId;
    }
    if (savedUserId) {
        userIdInput.value = savedUserId;
    }

    const explorerAvatarId = localStorage.getItem('explorer_avatarId');
    const explorerUserId   = localStorage.getItem('explorer_userId');
    if (explorerAvatarId) {
        localStorage.removeItem('explorer_avatarId');
        localStorage.removeItem('explorer_userId');
        avatarIdInput.value = explorerAvatarId;
        userIdInput.value   = explorerUserId || '';
        localStorage.setItem('avatarId', explorerAvatarId);
        if (explorerUserId) localStorage.setItem('userId', explorerUserId);
        setTimeout(() => loadAvatarInfo(), 80);
    }

    initLinuxSettings();
    initUnlockTabSettings();

    const savedHistory = localStorage.getItem('avatarHistory');
    if (savedHistory) {
        try {
            avatarHistory = JSON.parse(savedHistory);
        } catch (e) {
            console.error("Error parsing avatar history from localStorage:", e);
            avatarHistory = [];
        }
    }
    renderHistory();

    initFeedbackTab();
    initTitlebar();

    updateUnlockUI();
}

// ─── Feedback Tab ────────────────────────────────────────────────────────────

let _selectedFeedbackCategory = 'bug';

function initFeedbackTab() {
    _selectedFeedbackCategory = 'bug';
}

function selectFeedbackCategory(btn) {
    document.querySelectorAll('.feedback-cat-btn').forEach(b => b.classList.remove('active'));
    btn.classList.add('active');
    _selectedFeedbackCategory = btn.getAttribute('data-cat');
}

function submitFeedback() {
    const subject = document.getElementById('fb-subject')?.value.trim();
    const body    = document.getElementById('fb-body')?.value.trim();
    if (!subject || !body) {
        _showFbStatus('Please fill in a subject and message.', false);
        return;
    }
    const btn = document.getElementById('fb-submit-btn');
    if (btn) { btn.disabled = true; btn.innerHTML = '<i class="fa-solid fa-spinner fa-spin"></i> Sending...'; }
    sendMessageToDotNet({
        type:     'submitFeedback',
        category: _selectedFeedbackCategory,
        name:     document.getElementById('fb-name')?.value.trim()  || 'Anonymous',
        email:    document.getElementById('fb-email')?.value.trim() || '',
        subject,
        body
    });
}

function clearFeedbackForm() {
    ['fb-name','fb-email','fb-subject','fb-body'].forEach(id => {
        const el = document.getElementById(id);
        if (el) el.value = '';
    });
    _showFbStatus('', false, true);
}

function _showFbStatus(msg, success, hide) {
    const el  = document.getElementById('fb-status');
    const btn = document.getElementById('fb-submit-btn');
    if (btn) { btn.disabled = false; btn.innerHTML = '<i class="fa-solid fa-paper-plane"></i> Submit'; }
    if (!el) return;
    if (hide) { el.style.display = 'none'; return; }
    el.style.display = 'block';
    el.style.background    = success ? 'color-mix(in srgb, #27ae60 15%, var(--panel-bg-color))'
                                     : 'color-mix(in srgb, #c0392b 12%, var(--panel-bg-color))';
    el.style.border        = `1px solid ${success ? '#27ae60' : '#c0392b'}`;
    el.style.color         = 'var(--text-color)';
    el.textContent         = msg;
}

// ─── Window Controls ─────────────────────────────────────────────────────────
let serviceIsRunning = false;

let isMaximized = false;

function windowMinimize() { sendMessageToDotNet({ type: 'windowControl', cmd: 'minimize' }); }
function toggleMaximize() {
    isMaximized = !isMaximized;
    sendMessageToDotNet({ type: 'windowControl', cmd: isMaximized ? 'maximize' : 'restore' });
    const btn = document.getElementById('btn-maximize');
    if (btn) {
        const icon = btn.querySelector('i');
        if (icon) icon.className = isMaximized ? 'fa-regular fa-clone' : 'fa-regular fa-square';
        btn.title = isMaximized ? 'Restore' : 'Maximize';
    }
}
function windowClose() {
    if (serviceIsRunning) {
        const modal = document.getElementById('service-close-modal');
        if (modal) modal.style.display = 'flex';
    } else {
        sendMessageToDotNet({ type: 'closeApp', stopService: false });
    }
}
function closeWithStopService() {
    const modal = document.getElementById('service-close-modal');
    if (modal) modal.style.display = 'none';
    sendMessageToDotNet({ type: 'closeApp', stopService: true });
}
function closeLeaveService() {
    const modal = document.getElementById('service-close-modal');
    if (modal) modal.style.display = 'none';
    sendMessageToDotNet({ type: 'closeApp', stopService: false });
}

function initTitlebar() {
    const dragRegion = document.getElementById('titlebar-drag');
    if (!dragRegion) return;
    let dragging = false;
    let lastX = 0, lastY = 0;
    dragRegion.addEventListener('mousedown', (e) => {
        if (e.button !== 0 || e.target.closest('.titlebar-btn')) return;
        dragging = true;
        lastX = e.screenX;
        lastY = e.screenY;
        e.preventDefault();
    });
    document.addEventListener('mousemove', (e) => {
        if (!dragging) return;
        const dx = e.screenX - lastX;
        const dy = e.screenY - lastY;
        if (dx === 0 && dy === 0) return;
        lastX = e.screenX;
        lastY = e.screenY;
        sendMessageToDotNet({ type: 'windowControl', cmd: 'move', dx, dy });
    });
    document.addEventListener('mouseup', () => { dragging = false; });
}

// ─── Extras Accordion ────────────────────────────────────────────────────────
function toggleExtrasSection(id) {
    const body    = document.getElementById(id);
    const chevron = document.getElementById('chevron-' + id);
    if (!body) return;
    const isOpen = body.classList.contains('open');
    body.classList.toggle('open', !isOpen);
    if (chevron) chevron.classList.toggle('rotated', !isOpen);
}

// ─── Generic Accordion (acc-section / acc-body / acc-chevron) ────────────────
function toggleAccordion(id) {
    const body    = document.getElementById(id);
    const chevron = document.getElementById('chevron-' + id);
    if (!body) return;
    const isOpen = body.classList.contains('open');
    body.classList.toggle('open', !isOpen);
    if (chevron) chevron.classList.toggle('rotated', !isOpen);
}

function updateSidebarToggleIcon(isExpanded) {
    const icon = sidebarToggle.querySelector('i');
    if (icon) {
        icon.className = isExpanded ? 'fa-solid fa-arrow-left' : 'fa-solid fa-bars';
    }
}

// --- Button Click Functions ---
function viewAvatarModal() {
    const avatarId = avatarIdInput.value;
    if (!avatarId) {
        alert("Please enter an Avatar ID.");
        return;
    }
    currentModalAvatarId = avatarId;
    if (modalAvatarImg) modalAvatarImg.src = "https://placehold.co/400x300?text=Loading...";
    if (modalAvatarName) modalAvatarName.textContent = "Loading...";
    if (modalAvatarAuthor) modalAvatarAuthor.textContent = "Loading...";
    if (modalAvatarDesc) modalAvatarDesc.textContent = "Loading...";
    if (avatarInfoPopup) avatarInfoPopup.classList.add('show');

    sendMessageToDotNet({ type: 'fetchAvatarInfo', avatarId: avatarId });
}

function openInBrowser() {
    const avatarId = avatarIdInput.value;
    if (!avatarId) {
        alert("Please enter an Avatar ID.");
        return;
    }
    sendMessageToDotNet({ type: 'openInBrowser', avatarId: avatarId, userId: userIdInput.value });
}

function closeAvatarInfoPopup() {
    if (avatarInfoPopup) avatarInfoPopup.classList.remove('show');
    currentModalAvatarId = null;
}

function loadAvatarInfo() {
    const avatarId = avatarIdInput.value;
    const userId = userIdInput.value;

    if (!avatarId) {
        console.error("Avatar ID is required.");
        alert("Please enter an Avatar ID.");
        return;
    }

    console.log(`Loading info for Avatar: ${avatarId}, User: ${userId || 'N/A'}`);

    loadedAvatarIdSpan.textContent = avatarId;
    loadedUserIdSpan.textContent = censorUserIdToggle.checked ? '********' : (userId || 'N/A');

    // Reset PAW fields to loading
    if (loadedAvatarImg) loadedAvatarImg.src = "https://placehold.co/100x100?text=Loading...";
    if (loadedAvatarName) loadedAvatarName.textContent = "Loading...";
    if (loadedAvatarAuthor) loadedAvatarAuthor.textContent = "Loading...";

    isAvatarLoaded = true;
    window.loadedAvatarId = avatarId;
    window.loadedUserId = userId;

    updateUnlockUI();
    addAvatarToHistory(avatarId, userId);
    showToast(`Avatar loaded: ${avatarId}`, 'info', 'Avatar', 3500);

    // Fetch PAW Info for loaded panel
    sendMessageToDotNet({ type: 'fetchAvatarInfo', avatarId: avatarId });
}

function triggerPopup(actionType) {
    if (!isAvatarLoaded || !window.loadedAvatarId) {
        console.error("No avatar loaded. Cannot perform unlock action.");
        showNoAvatarWarning();
        return;
    }

    console.log(`${actionType} button clicked for Avatar: ${window.loadedAvatarId}`);

    // Only show popup if debug/verbose mode is enabled
    if (showBackendMessages) {
        popupTitle.textContent = `${actionType} Action`;
        popupMessage.textContent = `Calling .NET...`;
        actionPopup.classList.add('show');
    }

    const message = {
        type: 'unlockAction',
        action: actionType,
        avatarId: window.loadedAvatarId,
        userId: window.loadedUserId || null
    };
    _setUnlockButtonsDisabled(true);
    sendMessageToDotNet(message);
    showToast(`${actionType} started...`, 'info', 'Unlock', 3000);

    // Auto-switch to Logs tab
    const logsTabBtn = document.querySelector('.tab-button[data-tab="logs"]');
    if (logsTabBtn) {
        logsTabBtn.click();
    }
}

function openHelpUrl() {
    console.log("Requesting help URL from .NET...");
    const message = { type: 'openHelpUrl' };
    sendMessageToDotNet(message);
}

function triggerRestart(restartType) {
    if (!isAvatarLoaded || !window.loadedAvatarId) {
        console.error("No avatar loaded. Restart action might depend on loaded avatar.");
    }

    console.log(`Triggering restart: ${restartType}`);

    const message = { type: restartType };
    sendMessageToDotNet(message);

    // Only show popup if debug/verbose mode is enabled
    if (showBackendMessages) {
        popupTitle.textContent = restartType === 'restart-vr' ? 'Restart (VR)' : 'Restart';
        popupMessage.textContent = `Requesting ${popupTitle.textContent}...`;
        actionPopup.classList.add('show');
    }
}

// --- Custom / Linux / Windows Cache Settings Functions ---
function initLinuxSettings() {
    const savedLinuxPath = localStorage.getItem('customLinuxPath');
    if (savedLinuxPath && document.getElementById('custom-linux-cache-path')) {
        document.getElementById('custom-linux-cache-path').value = savedLinuxPath;
        sendLinuxSettingsToBackend(savedLinuxPath);
    }
    const savedWinPath = localStorage.getItem('customWindowsCachePath');
    if (savedWinPath && document.getElementById('custom-win-cache-path')) {
        document.getElementById('custom-win-cache-path').value = savedWinPath;
        sendWindowsCacheToBackend(savedWinPath);
    }
    initAutoUnlockSettings();
}

function saveLinuxSettings() {
    const linuxPath = document.getElementById('custom-linux-cache-path').value.trim();
    localStorage.setItem('customLinuxPath', linuxPath);
    sendLinuxSettingsToBackend(linuxPath);

    popupTitle.textContent = "Linux Settings Saved";
    popupMessage.textContent = "Custom cache path has been updated.";
    actionPopup.classList.add('show');
}

function resetLinuxSettings() {
    document.getElementById('custom-linux-cache-path').value = '';
    localStorage.removeItem('customLinuxPath');
    sendLinuxSettingsToBackend("");

    popupTitle.textContent = "Linux Settings Reset";
    popupMessage.textContent = "Now using default Proton cache path.";
    actionPopup.classList.add('show');
}

function sendLinuxSettingsToBackend(linuxPath) {
    sendMessageToDotNet({ type: 'linuxSettings', path: linuxPath });
}

function saveWindowsCacheSettings() {
    const path = document.getElementById('custom-win-cache-path')?.value.trim() || '';
    localStorage.setItem('customWindowsCachePath', path);
    sendWindowsCacheToBackend(path);
    popupTitle.textContent = 'Windows Cache Path Saved';
    popupMessage.textContent = 'Cache path updated.';
    actionPopup.classList.add('show');
}

function resetWindowsCacheSettings() {
    const el = document.getElementById('custom-win-cache-path');
    if (el) el.value = '';
    localStorage.removeItem('customWindowsCachePath');
    sendWindowsCacheToBackend('');
    popupTitle.textContent = 'Windows Cache Path Reset';
    popupMessage.textContent = 'Now using default Windows cache path.';
    actionPopup.classList.add('show');
}

function sendWindowsCacheToBackend(path) {
    sendMessageToDotNet({ type: 'windowsCacheSettings', path: path });
}

// --- Auto-Unlock Settings ---
function initAutoUnlockSettings() {
    const enabled = localStorage.getItem('autoUnlockEnabled') === 'true';
    const mode = localStorage.getItem('autoUnlockMode') || 'inapp';
    const interval = parseInt(localStorage.getItem('autoUnlockInterval') || '30', 10);

    const enabledToggle = document.getElementById('auto-unlock-enabled');
    const modeSelect = document.getElementById('auto-unlock-mode');
    const intervalInput = document.getElementById('auto-unlock-interval');

    if (enabledToggle) enabledToggle.checked = enabled;
    if (modeSelect) {
        modeSelect.value = mode;
        modeSelect.addEventListener('change', () => _updateAutoUnlockButtons(modeSelect.value, false));
    }
    if (intervalInput) intervalInput.value = interval;

    _updateAutoUnlockButtons(mode, enabled);
    sendMessageToDotNet({ type: 'autoUnlockSettings', enabled, mode, interval });
}

function _updateAutoUnlockButtons(mode, running) {
    const forceStop = document.getElementById('btn-force-stop');
    const viewLog   = document.getElementById('btn-view-log');
    if (forceStop) forceStop.style.display = running ? 'inline-flex' : 'none';
    if (viewLog)   viewLog.style.display   = (mode === 'service') ? 'inline-flex' : 'none';
}

function openServiceLog() {
    sendMessageToDotNet({ type: 'openServiceLog' });
}

function forceStopAutoUnlock() {
    sendMessageToDotNet({ type: 'stopService' });
    sendMessageToDotNet({ type: 'autoUnlockSettings', enabled: false, mode: 'inapp', interval: 30 });
    updateAutoUnlockStatusUI(false);
    showToast('Auto Unlock force stopped.', 'warn', 'Auto Unlock', 3000);
}

function updateAutoUnlockStatusUI(running) {
    serviceIsRunning = running;
    const mode = document.getElementById('auto-unlock-mode')?.value || 'inapp';
    const dot  = document.getElementById('auto-unlock-status-dot');
    const lbl  = document.getElementById('auto-unlock-status-label');
    if (dot) dot.style.color = running ? 'var(--accent-color)' : 'var(--border-color)';
    if (lbl) lbl.textContent = running ? 'Status: Running' : 'Status: Stopped';
    _updateAutoUnlockButtons(mode, running);
}

function saveAutoUnlockSettings() {
    const enabled  = document.getElementById('auto-unlock-enabled')?.checked || false;
    const mode     = document.getElementById('auto-unlock-mode')?.value || 'inapp';
    const interval = parseInt(document.getElementById('auto-unlock-interval')?.value || '30', 10);

    localStorage.setItem('autoUnlockEnabled', enabled);
    localStorage.setItem('autoUnlockMode', mode);
    localStorage.setItem('autoUnlockInterval', interval);

    // Apply in-app runner settings (starts/stops loop)
    sendMessageToDotNet({ type: 'autoUnlockSettings', enabled, mode, interval });

    // Always write service config so the service exe has fresh settings
    sendMessageToDotNet({
        type: 'writeServiceConfig', enabled, mode, interval,
        dbUrl: localStorage.getItem('customDbUrl') || '',
        dbPath: localStorage.getItem('customDbPath') || '',
        windowsCachePath: localStorage.getItem('customWindowsCachePath') || ''
    });

    if (mode === 'service') {
        if (enabled) {
            sendMessageToDotNet({ type: 'startService' });
            // Status dot updated via serviceStatus callback from C#
        } else {
            sendMessageToDotNet({ type: 'stopService' });
            updateAutoUnlockStatusUI(false);
        }
    } else {
        // In-app: we know it started synchronously
        updateAutoUnlockStatusUI(enabled);
    }

    showToast('Auto Unlock applied.', 'success', 'Auto Unlock', 2500);
}

function resetTheme() {
    console.log("Resetting to default theme (Dodger Blue)");
    localStorage.removeItem('themeType');
    localStorage.removeItem('themeName');
    colorPickers.forEach(picker => {
        const cssVar = picker.getAttribute('data-var');
        localStorage.removeItem(`customColor_${cssVar}`);
    });
    applyTheme(themes.dodgerblue);
}

// --- History / Recents Tab Functions ---
function renderHistory() {
    historyListContainer.innerHTML = '';
    if (avatarHistory.length === 0) {
        if (noHistoryMessage) noHistoryMessage.style.display = 'block';
    } else {
        if (noHistoryMessage) noHistoryMessage.style.display = 'none';
        avatarHistory.forEach(item => {
            const itemDiv = document.createElement('div');
            itemDiv.className = 'history-item';
            const formattedDate = new Date(item.timestamp).toLocaleString();
            const noteContent = item.note
                ? `<p class="history-item-note">${item.note}</p>`
                : `<p class="history-item-note placeholder">Looks like this avatar is a mystery. Add a note so your future self knows what's up!</p>`;

            itemDiv.innerHTML = `
                <div style="display: flex; flex-direction: column; align-items: center; gap: 15px; margin-bottom: 10px; text-align: center;">
                    <img src="${item.imageUrl || 'https://placehold.co/400x300?text=No+Image'}" alt="Thumb" style="width: 100%; max-width: 400px; height: 200px; object-fit: cover; border-radius: 8px;">
                    <div style="width: 100%; word-break: break-all;">
                        <h4 style="margin: 0 0 5px 0;">${item.name || item.avatarId}</h4>
                        <p style="margin: 0 0 5px 0;"><strong>Author:</strong> ${item.authorName || 'Unknown'}</p>
                        <p style="margin: 0 0 5px 0; font-size: 0.8em; color: var(--text-color); opacity: 0.8;"><strong>ID:</strong> ${item.avatarId}</p>
                        <p style="margin: 0;"><strong>User:</strong> ${item.userId || 'N/A'}</p>
                        <p style="margin: 0;"><small>Loaded: ${formattedDate}</small></p>
                    </div>
                </div>
                <div class="history-item-note-container">
                    ${noteContent}
                </div>
                <div class="history-item-actions">
                    <button class="action-button" onclick="loadFromHistory('${item.avatarId}', '${item.userId}')"><i class="fa-solid fa-upload"></i> Load</button>
                    <button class="action-button secondary-action" onclick="showNotePopup('${item.avatarId}')"><i class="fa-solid fa-pencil"></i> ${item.note ? 'Edit' : 'Add'} Note</button>
                    <button class="action-button secondary-action" onclick="deleteFromHistory(event, '${item.avatarId}')"><i class="fa-solid fa-trash"></i> Delete</button>
                </div>
            `;
            historyListContainer.appendChild(itemDiv);
        });
    }
}

function addAvatarToHistory(avatarId, userId) {
    let existingNote = null;
    let existingImageUrl = null;
    let existingName = null;
    let existingAuthor = null;

    const existingIndex = avatarHistory.findIndex(item => item.avatarId === avatarId);
    if (existingIndex > -1) {
        existingNote = avatarHistory[existingIndex].note;
        existingImageUrl = avatarHistory[existingIndex].imageUrl;
        existingName = avatarHistory[existingIndex].name;
        existingAuthor = avatarHistory[existingIndex].authorName;
        avatarHistory.splice(existingIndex, 1);
    }

    avatarHistory.unshift({
        avatarId: avatarId,
        userId: userId,
        timestamp: new Date().getTime(),
        note: existingNote,
        imageUrl: existingImageUrl,
        name: existingName,
        authorName: existingAuthor
    });

    if (avatarHistory.length > 50) {
        avatarHistory.pop();
    }

    localStorage.setItem('avatarHistory', JSON.stringify(avatarHistory));
    renderHistory();
}

function updateHistoryWithPawData(avatarId, name, authorName, imageUrl, description) {
    const item = avatarHistory.find(i => i.avatarId === avatarId);
    if (item) {
        item.name = name;
        item.authorName = authorName;
        item.imageUrl = imageUrl;
        if (!item.note && description && description !== 'Placeholder info (avatar not in API).') {
            item.note = description;
        }
        localStorage.setItem('avatarHistory', JSON.stringify(avatarHistory));
        renderHistory();
    }
}

function loadFromHistory(avatarId, userId) {
    avatarIdInput.value = avatarId;
    userIdInput.value = userId;

    localStorage.setItem('avatarId', avatarId);
    localStorage.setItem('userId', userId);

    document.querySelector('.tab-button[data-tab="avatar"]').click();
    loadAvatarInfo();
}

function showNotePopup(avatarId) {
    currentEditingAvatarId = avatarId;
    const historyItem = avatarHistory.find(item => item.avatarId === avatarId);
    if (historyItem) {
        noteTextarea.value = historyItem.note || '';
        notePopup.classList.add('show');
    }
}

function closeNotePopup() {
    notePopup.classList.remove('show');
    currentEditingAvatarId = null;
}

function saveNote() {
    if (currentEditingAvatarId) {
        const historyItem = avatarHistory.find(item => item.avatarId === currentEditingAvatarId);
        if (historyItem) {
            historyItem.note = noteTextarea.value.trim() ? noteTextarea.value : null;
            localStorage.setItem('avatarHistory', JSON.stringify(avatarHistory));
            renderHistory();
        }
    }
    closeNotePopup();
}

function deleteFromHistory(event, avatarId) {
    event.stopPropagation();
    avatarHistory = avatarHistory.filter(item => item.avatarId !== avatarId);
    localStorage.setItem('avatarHistory', JSON.stringify(avatarHistory));
    renderHistory();
}

function exportHistory() {
    if (avatarHistory.length === 0) {
        popupTitle.textContent = "Export Failed";
        popupMessage.textContent = "History is empty. Nothing to export.";
        actionPopup.classList.add('show');
        return;
    }
    const jsonString = JSON.stringify(avatarHistory, null, 2);
    const message = { type: 'exportHistory', payload: jsonString };
    sendMessageToDotNet(message);

    popupTitle.textContent = "Export Successful";
    popupMessage.textContent = "History has been sent to the application for exporting.";
    actionPopup.classList.add('show');
}

function showImportPopup() {
    importJsonTextarea.value = '';
    importErrorMessage.style.display = 'none';
    importJsonPopup.classList.add('show');
}

function closeImportPopup() {
    importJsonPopup.classList.remove('show');
}

function importHistory() {
    const jsonString = importJsonTextarea.value;
    if (!jsonString) {
        importErrorMessage.textContent = "Textarea is empty.";
        importErrorMessage.style.display = 'block';
        return;
    }

    try {
        const importedData = JSON.parse(jsonString);
        if (!Array.isArray(importedData) || !importedData.every(item => item.avatarId && item.timestamp)) {
            throw new Error("Invalid history format. Must be an array of objects with 'avatarId' and 'timestamp'.");
        }
        avatarHistory = importedData.map(item => ({
            avatarId: item.avatarId,
            userId: item.userId || null,
            timestamp: item.timestamp,
            note: item.note || null
        }));
        localStorage.setItem('avatarHistory', JSON.stringify(avatarHistory));
        renderHistory();
        closeImportPopup();
    } catch (e) {
        importErrorMessage.textContent = `Import failed: ${e.message}`;
        importErrorMessage.style.display = 'block';
    }
}

function closePopup() {
    if (actionPopup) {
        actionPopup.classList.remove('show');
    }
}

function checkPawStatus() {
    const apiStatusText = document.getElementById('api-status-text');
    if (apiStatusText) apiStatusText.textContent = 'Checking...';
    if (apiStatusIcon) {
        apiStatusIcon.classList.remove('online', 'offline');
        apiStatusIcon.classList.add('checking');
    }
    sendMessageToDotNet({ type: 'checkPawStatus' });
}

// --- Toast Notification System ---
const _toastIcons = {
    success: 'fa-solid fa-circle-check',
    error: 'fa-solid fa-circle-xmark',
    warn: 'fa-solid fa-triangle-exclamation',
    info: 'fa-solid fa-circle-info'
};

function showToast(message, type = 'info', title = '', duration = 4000) {
    if (!toastNotificationsEnabled) return;
    const container = document.getElementById('toast-container');
    if (!container) return;

    const toast = document.createElement('div');
    toast.className = `toast toast-${type}`;
    toast.style.position = 'relative';
    toast.innerHTML = `
        <i class="toast-icon ${_toastIcons[type] || _toastIcons.info}"></i>
        <div class="toast-body">
            ${title ? `<span class="toast-title">${title}</span>` : ''}
            <span class="toast-message">${message}</span>
        </div>
        <div class="toast-progress" style="animation-duration: ${duration}ms;"></div>
    `;

    container.appendChild(toast);

    const dismiss = () => {
        toast.classList.add('toast-exit');
        toast.addEventListener('animationend', () => toast.remove(), { once: true });
    };

    const timer = setTimeout(dismiss, duration);
    toast.addEventListener('click', () => { clearTimeout(timer); dismiss(); });
}

function appendAppLog(logData) {
    const firstEntry = appLogsContainer.querySelector('.log-entry');
    if (firstEntry && firstEntry.textContent.includes("Waiting for logs...")) {
        appLogsContainer.innerHTML = '';
    }

    const entryDiv = document.createElement('div');
    entryDiv.className = 'log-entry';

    let color = 'var(--text-color)';
    let icon = '';

    switch (logData.logType) {
        case 'success':
            color = '#00ff40';
            icon = '<i class="fa-solid fa-check-circle"></i>';
            break;
        case 'warn':
            color = '#ffcc00';
            icon = '<i class="fa-solid fa-triangle-exclamation"></i>';
            break;
        case 'error':
            color = '#ff002b';
            icon = '<i class="fa-solid fa-circle-xmark"></i>';
            break;
        case 'info':
        default:
            color = 'var(--text-color)';
            icon = '<i class="fa-solid fa-info-circle"></i>';
            break;
    }

    entryDiv.innerHTML = `
        <span style="color: #00ff5e; font-size: 0.9em;">[${logData.timestamp}]</span>
        <span style="color: #0084ff; font-weight: bold;">[${logData.task}]</span>
        <span style="color: ${color};">${icon} ${logData.message}</span>
    `;

    appLogsContainer.appendChild(entryDiv);
    appLogsContainer.scrollTop = appLogsContainer.scrollHeight;
}

function clearAppLogs() {
    appLogsContainer.innerHTML = '<div class="log-entry"><span class="log-system">[System]</span> Logs cleared.</div>';
}

function copyAppLogs() {
    const text = appLogsContainer.innerText;
    navigator.clipboard.writeText(text).then(() => {
        const btn = document.querySelector('button[onclick="copyAppLogs()"]');
        const originalHTML = btn.innerHTML;
        btn.innerHTML = '<i class="fa-solid fa-check"></i> Copied!';
        setTimeout(() => { btn.innerHTML = originalHTML; }, 2000);
    }).catch(err => {
        console.error('Failed to copy logs: ', err);
    });
}

// --- .NET Response Popup Functions ---
function showNetResponsePopup() {
    if (netResponsePopup) {
        netResponseContent.textContent = netLog.join('\n');
        netResponsePopup.classList.add('show');
    }
}

function closeNetResponsePopup() {
    if (netResponsePopup) {
        netResponsePopup.classList.remove('show');
    }
}

function clearNetLog() {
    netLog = ["Log cleared."];
    if (netResponseContent) {
        netResponseContent.textContent = netLog[0];
    }
}

// --- Unlock UI Functions ---
function updateUnlockUI() {
    const explorerNoAvi  = document.getElementById('explorer-no-avatar');
    const explorerLaunch = document.getElementById('explorer-launch');
    const explorerLabel  = document.getElementById('explorer-avatar-label');
    if (isAvatarLoaded) {
        loadedAvatarInfoPanel.style.display = 'block';
        unlockWarningPanel.style.display = 'none';
        unlockActionButtons.forEach(btn => btn.disabled = false);
        if (explorerNoAvi)  explorerNoAvi.style.display  = 'none';
        if (explorerLaunch) explorerLaunch.style.display = 'block';
        if (explorerLabel && window.loadedAvatarId) explorerLabel.textContent = window.loadedAvatarId;
    } else {
        loadedAvatarInfoPanel.style.display = 'none';
        unlockWarningPanel.style.display = 'block';
        unlockActionButtons.forEach(btn => btn.disabled = true);
        if (explorerNoAvi)  explorerNoAvi.style.display  = 'block';
        if (explorerLaunch) explorerLaunch.style.display = 'none';
    }
}

function openExplorer() {
    if (!isAvatarLoaded || !window.loadedAvatarId) return;
    localStorage.setItem('explorer_avatarId', window.loadedAvatarId);
    localStorage.setItem('explorer_userId',   window.loadedUserId || '');
    sendMessageToDotNet({
        type:     'openExplorer',
        avatarId: window.loadedAvatarId,
        userId:   window.loadedUserId || ''
    });
}

function showUnloadConfirmation() {
    if (confirmUnloadPopup) {
        confirmUnloadPopup.classList.add('show');
    }
}

function confirmUnloadAvatar() {
    console.log("Unloading avatar...");
    isAvatarLoaded = false;
    loadedAvatarId = null;
    loadedUserId = null;
    updateUnlockUI();
    cancelUnloadAvatar();
}

function cancelUnloadAvatar() {
    if (confirmUnloadPopup) {
        confirmUnloadPopup.classList.remove('show');
    }
}

// --- No Avatar Warning Popup ---
function showNoAvatarWarning() {
    popupTitle.textContent = "No Avatar Loaded";
    popupMessage.innerHTML = '<i class="fa-solid fa-triangle-exclamation" style="color: #ffcc00; margin-right: 8px;"></i> Please load an avatar from the <strong>Avatar</strong> tab first before performing unlock actions.';
    actionPopup.classList.add('show');
}

// --- Unlock Notice Popup Logic ---
function closeUnlockNotice() {
    if (unlockNoticePopup) {
        unlockNoticePopup.classList.remove('show');
        if (noticeTimerInterval) {
            clearInterval(noticeTimerInterval);
            noticeTimerInterval = null;
            console.log("Unlock notice popup closed manually.");
        }
    }
}

function showUnlockNotice() {
    if (unlockNoticePopup && !noticeShownThisSession) {
        noticeShownThisSession = true;
        localStorage.setItem('unlockNoticeShown', 'true');
        console.log("Showing unlock notice popup.");

        let secondsLeft = 20;
        noticeTimerSpan.textContent = secondsLeft;
        unlockNoticePopup.classList.add('show');

        if (noticeTimerInterval) {
            clearInterval(noticeTimerInterval);
        }

        noticeTimerInterval = setInterval(() => {
            secondsLeft--;
            if (noticeTimerSpan) {
                noticeTimerSpan.textContent = secondsLeft;
            }
            if (secondsLeft <= 0) {
                closeUnlockNotice();
            }
        }, 1000);
    }
}

// --- Clear Cache & Reload ---       
function clearCacheAndReload() {
    const confirmation = confirm("Are you sure you want to clear locally stored settings (theme, sidebar state, etc.) and reload the GUI? This action cannot be undone.");
    if (confirmation) {
        console.log("Clearing local storage and reloading...");
        try {
            localStorage.removeItem('sidebarExpanded');
            localStorage.removeItem('animationsEnabled');
            localStorage.removeItem('censorUserId');
            localStorage.removeItem('themeType');
            localStorage.removeItem('themeName');
            localStorage.removeItem('unlockNoticeShown');
            localStorage.removeItem('avatarHistory');
            colorPickers.forEach(picker => {
                const cssVar = picker.getAttribute('data-var');
                localStorage.removeItem(`customColor_${cssVar}`);
            });

            window.location.reload();
        } catch (error) {
            console.error("Error clearing local storage or reloading:", error);
            alert("An error occurred while trying to clear cache and reload.");
        }
    } else {
        console.log("Clear cache and reload cancelled.");
    }
}

// --- Clear Input Fields Logic ---
function clearInputFields() {
    console.log("Clearing Avatar and User ID fields and cache.");
    avatarIdInput.value = '';
    userIdInput.value = '';
    localStorage.removeItem('avatarId');
    localStorage.removeItem('userId');
}

// --- DOM Ready & Event Listeners ---
document.addEventListener('DOMContentLoaded', function () {
    // Cache all DOM elements
    appWrapper = document.getElementById('app-wrapper');
    tabButtons = document.querySelectorAll('.tab-button');
    tabPanels = document.querySelectorAll('.tab-panel');
    animationsToggle = document.getElementById('animations-toggle');
    userIdInput = document.getElementById('user-id');
    censorUserIdToggle = document.getElementById('censor-user-id');
    avatarIdInput = document.getElementById('avatar-id');
    responseSpan = document.getElementById('dotnet-response');
    colorPickers = document.querySelectorAll('input[type="color"]');
    htmlElement = document.documentElement;
    actionPopup = document.getElementById('action-popup');
    popupTitle = document.getElementById('popup-title');
    popupMessage = document.getElementById('popup-message');
    unlockNoticePopup = document.getElementById('unlock-notice-popup');
    noticeTimerSpan = document.getElementById('notice-timer');
    sidebarToggle = document.getElementById('sidebar-toggle');
    loadedAvatarInfoPanel = document.getElementById('loaded-avatar-info-panel');
    loadedAvatarIdSpan = document.getElementById('loaded-avatar-id');
    loadedUserIdSpan = document.getElementById('loaded-user-id');
    unlockActionButtons = document.querySelectorAll('.unlock-action-btn');
    unlockWarningPanel = document.getElementById('unlock-warning-panel');
    confirmUnloadPopup = document.getElementById('confirm-unload-popup');
    presetButtons = document.querySelectorAll('.preset-btn');
    historyListContainer = document.getElementById('history-list-container');
    noHistoryMessage = document.getElementById('no-history-message');
    importJsonPopup = document.getElementById('import-json-popup');
    importJsonTextarea = document.getElementById('import-json-textarea');
    importErrorMessage = document.getElementById('import-error-message');
    netResponsePopup = document.getElementById('net-response-popup');
    netResponseContent = document.getElementById('net-response-content');
    notePopup = document.getElementById('note-popup');
    noteTextarea = document.getElementById('note-textarea');
    appLogsContainer = document.getElementById('app-logs-container');
    backendMessagesToggle = document.getElementById('backend-messages-toggle');
    apiStatusIcon = document.getElementById('api-status-icon');
    avatarInfoPopup = document.getElementById('avatar-info-popup');
    modalAvatarImg = document.getElementById('modal-avatar-img');
    modalAvatarName = document.getElementById('modal-avatar-name');
    modalAvatarAuthor = document.getElementById('modal-avatar-author');
    modalAvatarDesc = document.getElementById('modal-avatar-desc');
    loadedAvatarImg = document.getElementById('loaded-avatar-img');
    loadedAvatarName = document.getElementById('loaded-avatar-name');
    loadedAvatarAuthor = document.getElementById('loaded-avatar-author');
});

// --- Window Load Event ---
window.addEventListener('load', () => {
    initialize();
    initCustomDbSettings();

    loadTranslations().then(() => {
        const languageSelect = document.getElementById('language-select');
        if (languageSelect) {
            languageSelect.value = localStorage.getItem('appLanguage') || 'en';
            languageSelect.addEventListener('change', (e) => {
                updateLanguage(e.target.value);
            });
            updateLanguage(languageSelect.value);
        }
    });


    // --- Sidebar Toggle Logic ---
    sidebarToggle.addEventListener('click', () => {
        const isExpanded = appWrapper.classList.toggle('sidebar-expanded');
        localStorage.setItem('sidebarExpanded', isExpanded);
        updateSidebarToggleIcon(isExpanded);
    });

    // --- Tab Switching Logic ---
    tabButtons.forEach(button => {
        button.addEventListener('click', () => {
            tabButtons.forEach(btn => btn.classList.remove('active'));
            tabPanels.forEach(panel => panel.classList.remove('active'));

            button.classList.add('active');
            const tabId = button.getAttribute('data-tab');
            const targetPanel = document.getElementById(`${tabId}-tab`);
            if (targetPanel) {
                targetPanel.classList.add('active');
            }

            if (tabId === 'unlock' && !noticeShownThisSession) {
                showUnlockNotice();
            }
        });
    });

    // --- Settings Logic ---
    animationsToggle.addEventListener('change', (event) => {
        const enableAnimations = event.target.checked;
        document.body.classList.toggle('no-animations', !enableAnimations);
        localStorage.setItem('animationsEnabled', JSON.stringify(enableAnimations));
        console.log(`Animations enabled: ${enableAnimations}`);
    });

    censorUserIdToggle.addEventListener('change', (event) => {
        const censor = event.target.checked;
        userIdInput.type = censor ? 'password' : 'text';
        localStorage.setItem('censorUserId', JSON.stringify(censor));
        console.log(`Censor User ID: ${censor}`);
    });

    // Backend messages toggle
    const savedBackendMsgPref = localStorage.getItem('showBackendMessages');
    if (savedBackendMsgPref !== null) {
        showBackendMessages = JSON.parse(savedBackendMsgPref);
        backendMessagesToggle.checked = showBackendMessages;
    }
    backendMessagesToggle.addEventListener('change', (event) => {
        showBackendMessages = event.target.checked;
        localStorage.setItem('showBackendMessages', JSON.stringify(showBackendMessages));
    });

    // Toast notifications toggle
    const toastToggle = document.getElementById('toast-notifications-toggle');
    const savedToastPref = localStorage.getItem('toastNotificationsEnabled');
    if (savedToastPref !== null) {
        toastNotificationsEnabled = JSON.parse(savedToastPref);
        if (toastToggle) toastToggle.checked = toastNotificationsEnabled;
    }
    if (toastToggle) {
        toastToggle.addEventListener('change', (event) => {
            toastNotificationsEnabled = event.target.checked;
            localStorage.setItem('toastNotificationsEnabled', JSON.stringify(toastNotificationsEnabled));
        });
    }

    colorPickers.forEach(picker => {
        picker.addEventListener('input', (event) => {
            const cssVar = event.target.getAttribute('data-var');
            const newColor = event.target.value;
            htmlElement.style.setProperty(cssVar, newColor);
            if (cssVar === '--accent-color') {
                updateAccentRgbVar(newColor);
            }
            localStorage.setItem('themeType', 'custom');
            localStorage.setItem(`customColor_${cssVar}`, newColor);
            localStorage.removeItem('themeName');
            console.log(`Saved custom color ${cssVar}: ${newColor}`);
        });
    });

    // Add Preset Button Listeners
    presetButtons.forEach(button => {
        button.addEventListener('click', (event) => {
            const presetName = event.target.getAttribute('data-preset');
            if (themes[presetName]) {
                console.log(`Applying preset theme: ${presetName}`);
                const fullTheme = { ...themes.dodgerblue, ...themes[presetName] };
                applyTheme(fullTheme, true);
                localStorage.setItem('themeType', 'preset');
                localStorage.setItem('themeName', presetName);
                colorPickers.forEach(picker => {
                    const cssVar = picker.getAttribute('data-var');
                    localStorage.removeItem(`customColor_${cssVar}`);
                });
            }
        });
    });

    // --- Disable Context Menu & Dev Tools ---
    document.addEventListener('contextmenu', event => {
        console.log("Context menu blocked.");
        event.preventDefault();
    });

    document.addEventListener('keydown', event => {
        if (event.key === 'F12') {
            console.log("F12 blocked.");
            event.preventDefault();
        }
        if (event.ctrlKey && event.shiftKey && (
            event.key === 'I' || event.key === 'i' ||
            event.key === 'J' || event.key === 'j' ||
            event.key === 'C' || event.key === 'c'
        )) {
            console.log(`Ctrl+Shift+${event.key.toUpperCase()} blocked.`);
            event.preventDefault();
        }
    });

    // --- Input Caching ---
    avatarIdInput.addEventListener('input', (event) => {
        localStorage.setItem('avatarId', event.target.value);
    });

    userIdInput.addEventListener('input', (event) => {
        localStorage.setItem('userId', event.target.value);
    });

    // --- Photino Message Receiver ---
    if (window.external && typeof window.external.receiveMessage === 'function') {
        window.external.receiveMessage(message => {
            // Try parsing as JSON first for logs and progress
            try {
                const data = JSON.parse(message);
                if (data && data.type === 'log') {
                    appendAppLog(data);
                    return;
                }
                if (data && data.type === 'downloadProgress') {
                    updateDownloadProgress(data.progress, data.status, data.title);
                    return;
                }
                if (data && data.type === 'downloadComplete') {
                    closeDownloadProgress();
                    return;
                }
                if (data && data.type === 'pawStatus') {
                    const isOnline = data.isOnline;
                    const apiStatusText = document.getElementById('api-status-text');
                    if (apiStatusIcon) {
                        apiStatusIcon.classList.remove('online', 'offline', 'checking');
                        apiStatusIcon.classList.add(isOnline ? 'online' : 'offline');
                    }
                    if (apiStatusText) {
                        apiStatusText.textContent = isOnline ? 'Online' : 'Offline';
                    }
                    if (!isOnline) {
                        showToast('PAW API is unreachable. Avatar info may be unavailable.', 'warn', 'API Status', 6000);
                    }
                    return;
                }
                if (data && data.type === 'serviceStatus') {
                    updateAutoUnlockStatusUI(data.running);
                    return;
                }
                if (data && data.type === 'adbDeviceList') {
                    _renderAdbDevices(data.devices || []);
                    return;
                }
                if (data && data.type === 'adbScanResult') {
                    _handleAdbScanResult(data);
                    return;
                }
                if (data && data.type === 'adbPathResult') {
                    _handleAdbPathResult(data);
                    return;
                }
                if (data && data.type === 'unlockComplete') {
                    _setUnlockButtonsDisabled(false);
                    return;
                }
                if (data && data.type === 'cacheSizeResult') {
                    _handleCacheSizeResult(data);
                    return;
                }
                if (data && data.type === 'cacheDeleteResult') {
                    _handleCacheDeleteResult(data);
                    return;
                }
                if (data && data.type === 'appInfo') {
                    const el = document.getElementById('titlebar-version');
                    if (el && data.version) el.textContent = 'v' + data.version;
                    const updVerEl = document.getElementById('upd-current-ver');
                    if (updVerEl && data.version) updVerEl.textContent = 'v' + data.version;
                    if (data.devMode) _initDevMode(data);
                    return;
                }
                if (data && data.type === 'updateCheckResult') {
                    _handleUpdateCheckResult(data);
                    return;
                }
                if (data && data.type === 'feedbackResult') {
                    _showFbStatus(data.message, data.success);
                    if (data.success) clearFeedbackForm();
                    return;
                }
                if (data && data.type === 'pawAvatarInfo') {
                    const found = data.found;
                    const name = found ? (data.name || 'Unknown') : 'Not Found in PAW';
                    const author = found ? (data.authorName || 'Unknown') : 'Not Found';
                    const desc = found ? (data.description || 'No description provided.') : 'Placeholder info (avatar not in API).';
                    const imgUrl = found ? (data.imageUrl || 'https://placehold.co/400x300?text=No+Image') : 'https://placehold.co/400x300?text=Not+Found';

                    // Update Modal
                    if (currentModalAvatarId === data.avatarId) {
                        if (modalAvatarImg) modalAvatarImg.src = imgUrl;
                        if (modalAvatarName) modalAvatarName.textContent = name;
                        if (modalAvatarAuthor) modalAvatarAuthor.textContent = author;
                        if (modalAvatarDesc) modalAvatarDesc.textContent = desc;
                    }
                    // Update Loaded Avatar Panel
                    if (window.loadedAvatarId === data.avatarId) {
                        if (loadedAvatarImg) loadedAvatarImg.src = imgUrl;
                        if (loadedAvatarName) loadedAvatarName.textContent = name;
                        if (loadedAvatarAuthor) loadedAvatarAuthor.textContent = author;
                    }
                    // Update History
                    updateHistoryWithPawData(data.avatarId, name, author, imgUrl, desc);
                    // Toast on result
                    if (found) {
                        showToast(`${name} by ${author}`, 'success', 'Avatar Found', 4000);
                    } else {
                        showToast(`Avatar not found in PAW database.`, 'warn', 'PAW Lookup', 4000);
                    }
                    return;
                }
            } catch (e) {
                // Not JSON or failed to parse, treat as plain text
            }

            console.log(`Message received from .NET: ${message}`);

            const timestamp = new Date().toLocaleTimeString();
            const logEntry = `[${timestamp}] ${message}`;

            if (netLog.length === 1 && netLog[0] === "Waiting for messages from .NET...") {
                netLog = [logEntry];
            } else {
                netLog.push(logEntry);
            }

            if (netResponseContent) {
                netResponseContent.textContent = netLog.join('\n');
                netResponseContent.scrollTop = netResponseContent.scrollHeight;
            }

            if (message === "ActionCompleteClosePopup") {
                closePopup();
            }
        });
    }
});

// --- Download Progress Popup Functions ---
function showDownloadProgress(title) {
    const popup = document.getElementById('download-progress-popup');
    const titleSpan = document.getElementById('download-title');
    const progressBar = document.getElementById('download-progress-bar');
    const statusText = document.getElementById('download-status');

    if (popup) {
        if (titleSpan) titleSpan.textContent = title || 'Downloading...';
        if (progressBar) progressBar.style.width = '0%';
        if (statusText) statusText.textContent = 'Preparing download...';
        popup.classList.add('show');
    }
}

function updateDownloadProgress(progress, status, title) {
    const popup = document.getElementById('download-progress-popup');
    const titleSpan = document.getElementById('download-title');
    const progressBar = document.getElementById('download-progress-bar');
    const statusText = document.getElementById('download-status');

    // Show popup if not already visible
    if (popup && !popup.classList.contains('show')) {
        popup.classList.add('show');
    }

    if (titleSpan && title) titleSpan.textContent = title;
    if (progressBar) progressBar.style.width = `${progress}%`;
    if (statusText) statusText.textContent = status || `${progress}%`;
}

function closeDownloadProgress() {
    const popup = document.getElementById('download-progress-popup');
    if (popup) {
        popup.classList.remove('show');
    }
}

// --- Custom Database Settings ---
let customDbUrlInput, customDbPathInput;

function initCustomDbSettings() {
    customDbUrlInput = document.getElementById('custom-db-url');
    customDbPathInput = document.getElementById('custom-db-path');

    // Load saved settings
    const savedUrl = localStorage.getItem('customDbUrl');
    const savedPath = localStorage.getItem('customDbPath');

    if (savedUrl && customDbUrlInput) customDbUrlInput.value = savedUrl;
    if (savedPath && customDbPathInput) customDbPathInput.value = savedPath;
}

function saveCustomDbSettings() {
    const url = customDbUrlInput ? customDbUrlInput.value.trim() : '';
    const path = customDbPathInput ? customDbPathInput.value.trim() : '';

    localStorage.setItem('customDbUrl', url);
    localStorage.setItem('customDbPath', path);

    // Send to .NET backend
    const message = {
        type: 'customDbSettings',
        url: url,
        path: path
    };
    sendMessageToDotNet(message);

    // Show confirmation
    popupTitle.textContent = "Settings Saved";
    popupMessage.innerHTML = '<i class="fa-solid fa-check-circle" style="color: #2ECC71; margin-right: 8px;"></i> Custom database settings have been saved.';
    actionPopup.classList.add('show');

    console.log(`Custom DB settings saved - URL: ${url}, Path: ${path}`);
}

function resetCustomDbSettings() {
    if (customDbUrlInput) customDbUrlInput.value = '';
    if (customDbPathInput) customDbPathInput.value = '';

    localStorage.removeItem('customDbUrl');
    localStorage.removeItem('customDbPath');

    // Notify backend to use default
    const message = {
        type: 'customDbSettings',
        url: '',
        path: ''
    };
    sendMessageToDotNet(message);

    popupTitle.textContent = "Settings Reset";
    popupMessage.innerHTML = '<i class="fa-solid fa-rotate-left" style="color: var(--accent-color); margin-right: 8px;"></i> Database settings reset to default.';
    actionPopup.classList.add('show');

    console.log("Custom DB settings reset to default");
}

// ─── Unlock Settings "2" (mirrored in Unlock tab accordion) ─────────────────
function saveCustomDbSettings2() {
    const url  = document.getElementById('custom-db-url2')?.value.trim()  || '';
    const path = document.getElementById('custom-db-path2')?.value.trim() || '';
    localStorage.setItem('customDbUrl',  url);
    localStorage.setItem('customDbPath', path);
    sendMessageToDotNet({ type: 'customDbSettings', url, path });
    // keep settings tab fields in sync
    const u = document.getElementById('custom-db-url');
    const p = document.getElementById('custom-db-path');
    if (u) u.value = url;
    if (p) p.value = path;
    showToast('Database settings saved.', 'success', 'Settings', 2500);
}
function resetCustomDbSettings2() {
    ['custom-db-url2','custom-db-path2'].forEach(id => { const el = document.getElementById(id); if (el) el.value = ''; });
    localStorage.removeItem('customDbUrl');
    localStorage.removeItem('customDbPath');
    sendMessageToDotNet({ type: 'customDbSettings', url: '', path: '' });
    const u = document.getElementById('custom-db-url');
    const p = document.getElementById('custom-db-path');
    if (u) u.value = '';
    if (p) p.value = '';
    showToast('Database settings reset to default.', 'info', 'Settings', 2500);
}
function saveWindowsCacheSettings2() {
    const path = document.getElementById('custom-win-cache-path2')?.value.trim() || '';
    localStorage.setItem('customWindowsCachePath', path);
    sendMessageToDotNet({ type: 'windowsCacheSettings', path });
    const el = document.getElementById('custom-win-cache-path');
    if (el) el.value = path;
    showToast('Windows cache path saved.', 'success', 'Settings', 2500);
}
function resetWindowsCacheSettings2() {
    const el2 = document.getElementById('custom-win-cache-path2');
    if (el2) el2.value = '';
    localStorage.removeItem('customWindowsCachePath');
    sendMessageToDotNet({ type: 'windowsCacheSettings', path: '' });
    const el = document.getElementById('custom-win-cache-path');
    if (el) el.value = '';
    showToast('Windows cache path reset to default.', 'info', 'Settings', 2500);
}
function saveLinuxSettings2() {
    const path = document.getElementById('custom-linux-cache-path2')?.value.trim() || '';
    localStorage.setItem('customLinuxPath', path);
    sendMessageToDotNet({ type: 'linuxSettings', path });
    const el = document.getElementById('custom-linux-cache-path');
    if (el) el.value = path;
    showToast('Linux cache path saved.', 'success', 'Settings', 2500);
}
function resetLinuxSettings2() {
    const el2 = document.getElementById('custom-linux-cache-path2');
    if (el2) el2.value = '';
    localStorage.removeItem('customLinuxPath');
    sendMessageToDotNet({ type: 'linuxSettings', path: '' });
    const el = document.getElementById('custom-linux-cache-path');
    if (el) el.value = '';
    showToast('Linux cache path reset to default.', 'info', 'Settings', 2500);
}

/** Populate the Unlock-tab "2" fields from localStorage on init */
function initUnlockTabSettings() {
    const dbUrl  = localStorage.getItem('customDbUrl')  || '';
    const dbPath = localStorage.getItem('customDbPath') || '';
    const winPath   = localStorage.getItem('customWindowsCachePath') || '';
    const linuxPath = localStorage.getItem('customLinuxPath') || '';
    const set = (id, val) => { const el = document.getElementById(id); if (el) el.value = val; };
    set('custom-db-url2',          dbUrl);
    set('custom-db-path2',         dbPath);
    set('custom-win-cache-path2',  winPath);
    set('custom-linux-cache-path2',linuxPath);
}

// ─── ADB / Quest-Android Unlock ──────────────────────────────────────────────
let _adbSelectedDevice = null;

function adbRefreshDevices() {
    const btn = document.getElementById('btn-adb-refresh');
    if (btn) { btn.disabled = true; btn.innerHTML = '<i class="fa-solid fa-rotate fa-spin"></i> Scanning…'; }
    document.getElementById('adb-status').textContent = 'Looking for connected devices…';
    document.getElementById('adb-device-count').textContent = '';
    sendMessageToDotNet({ type: 'adbGetDevices' });
}

function _renderAdbDevices(devices) {
    const btn = document.getElementById('btn-adb-refresh');
    if (btn) { btn.disabled = false; btn.innerHTML = '<i class="fa-solid fa-rotate"></i> Refresh Devices'; }
    const list = document.getElementById('adb-device-list');
    const controls = document.getElementById('adb-controls');
    const countSpan = document.getElementById('adb-device-count');
    const statusEl  = document.getElementById('adb-status');

    if (!list) return;

    if (!devices || devices.length === 0) {
        list.innerHTML = '<p style="font-size:0.88em; opacity:0.5; font-style:italic;">No ADB devices found. Make sure USB Debugging is enabled and the device is connected.</p>';
        if (controls) controls.style.display = 'none';
        _adbSelectedDevice = null;
        if (countSpan) countSpan.textContent = '';
        if (statusEl) statusEl.textContent = 'No devices found.';
        return;
    }

    if (countSpan) countSpan.textContent = `${devices.length} device(s) found`;
    if (statusEl) statusEl.textContent = '';

    list.innerHTML = '';
    const readyDevices = devices.filter(d => d.ready);

    devices.forEach(d => {
        const row = document.createElement('div');
        row.className = 'adb-device-row' + (d.ready ? '' : ' adb-device-disabled');
        row.dataset.deviceId = d.id;
        row.innerHTML = `
            <span class="adb-device-icon"><i class="fa-solid fa-${d.ready ? 'mobile-screen-button' : 'circle-exclamation'}" style="color:${d.ready ? 'var(--accent-color)' : '#f0c040'};"></i></span>
            <span class="adb-device-name">${d.model}</span>
            <span class="adb-device-id">${d.id}</span>
            <span class="adb-device-status adb-status-${d.status}">${d.status}</span>`;
        if (d.ready) {
            row.style.cursor = 'pointer';
            row.addEventListener('click', () => _selectAdbDevice(d.id, row));
        }
        list.appendChild(row);
    });

    // Auto-select if only one ready device
    if (readyDevices.length === 1) {
        const row = list.querySelector(`[data-device-id="${readyDevices[0].id}"]`);
        _selectAdbDevice(readyDevices[0].id, row);
    }
}

function _selectAdbDevice(deviceId, rowEl) {
    _adbSelectedDevice = deviceId;
    // Clear selection highlight from all rows
    document.querySelectorAll('.adb-device-row').forEach(r => r.classList.remove('adb-device-selected'));
    if (rowEl) rowEl.classList.add('adb-device-selected');
    const controls = document.getElementById('adb-controls');
    if (controls) controls.style.display = 'block';
    document.getElementById('adb-status').textContent = `Selected: ${deviceId}`;
    // Hide previous path/result
    const pathDiv = document.getElementById('adb-path-result');
    const resultDiv = document.getElementById('adb-scan-result');
    if (pathDiv) pathDiv.style.display = 'none';
    if (resultDiv) resultDiv.style.display = 'none';
}

function adbScanSelected() {
    if (!_adbSelectedDevice) { showToast('No device selected.', 'warn', 'ADB', 3000); return; }
    const btn = document.getElementById('btn-adb-scan');
    if (btn) { btn.disabled = true; btn.innerHTML = '<i class="fa-solid fa-rotate fa-spin"></i> Scanning…'; }
    document.getElementById('adb-status').textContent = 'Scanning device — this may take a moment…';
    const resultDiv = document.getElementById('adb-scan-result');
    if (resultDiv) resultDiv.style.display = 'none';
    sendMessageToDotNet({ type: 'adbScanDevice', deviceId: _adbSelectedDevice });
    // Auto-switch to Logs tab
    const logsTabBtn = document.querySelector('.tab-button[data-tab="logs"]');
    if (logsTabBtn) logsTabBtn.click();
}

function adbFindPath() {
    if (!_adbSelectedDevice) { showToast('No device selected.', 'warn', 'ADB', 3000); return; }
    const btn = document.getElementById('btn-adb-find-path');
    if (btn) { btn.disabled = true; btn.innerHTML = '<i class="fa-solid fa-rotate fa-spin"></i> Searching…'; }
    document.getElementById('adb-status').textContent = 'Searching for VRChat data path…';
    sendMessageToDotNet({ type: 'adbFindPath', deviceId: _adbSelectedDevice });
}

function _handleAdbScanResult(data) {
    const btn = document.getElementById('btn-adb-scan');
    if (btn) { btn.disabled = false; btn.innerHTML = '<i class="fa-solid fa-magnifying-glass"></i> Scan &amp; Unlock'; }
    const resultDiv = document.getElementById('adb-scan-result');
    const resultText = document.getElementById('adb-scan-result-text');
    const statusEl = document.getElementById('adb-status');
    if (resultDiv) {
        resultDiv.style.display = 'flex';
        resultDiv.classList.toggle('adb-result-error', !data.success);
        const icon = resultDiv.querySelector('i');
        if (icon) icon.className = data.success ? 'fa-solid fa-circle-check' : 'fa-solid fa-circle-xmark';
        if (icon) icon.style.color = data.success ? '#2ECC71' : '#e74c3c';
    }
    if (resultText) resultText.textContent = data.message || '';
    if (statusEl) statusEl.textContent = '';
    if (data.success) showToast(data.message, 'success', 'ADB Unlock', 5000);
    else showToast('ADB scan failed: ' + data.message, 'error', 'ADB', 5000);
}

function _handleAdbPathResult(data) {
    const btn = document.getElementById('btn-adb-find-path');
    if (btn) { btn.disabled = false; btn.innerHTML = '<i class="fa-solid fa-folder-open"></i> Find VRChat Path'; }
    const pathDiv = document.getElementById('adb-path-result');
    const pathText = document.getElementById('adb-path-text');
    const statusEl = document.getElementById('adb-status');
    if (pathDiv) pathDiv.style.display = data.found ? 'block' : 'none';
    if (pathText) pathText.textContent = data.found ? data.path : 'VRChat data not found on device.';
    if (statusEl) statusEl.textContent = data.found ? '' : 'VRChat data not found. Ensure VRChat has been launched on the device.';
    if (!data.found) showToast('VRChat data path not found on device.', 'warn', 'ADB', 4000);
}

// --- Localization Logic ---
let translations = {};
let _translationsLoaded = false;
let _pendingLang = null;

function loadTranslations() {
    return fetch('translations.json')
        .then(r => r.json())
        .then(data => {
            translations = data;
            _translationsLoaded = true;
            if (_pendingLang) {
                updateLanguage(_pendingLang);
                _pendingLang = null;
            }
        })
        .catch(err => {
            console.error('Failed to load translations.json:', err);
            _translationsLoaded = true;
        });
}



let currentLang = 'en';

function updateLanguage(lang) {
    if (!_translationsLoaded) {
        _pendingLang = lang;
        return;
    }
    if (!translations[lang]) lang = 'en';
    currentLang = lang;
    localStorage.setItem('appLanguage', lang);
    const langDict = translations[lang];
    if (!langDict) return;

    for (const selector in langDict) {
        const el = document.querySelector(selector);
        if (el) {
            el.innerHTML = langDict[selector];
        }
    }
}
function openAppDir() { sendMessageToDotNet({ action: 'openAppDirectory' }); }

// --- Cache Management ---
function checkCacheSize() {
    const btn = document.getElementById('btn-cache-check');
    if (btn) { btn.disabled = true; btn.innerHTML = '<i class="fa-solid fa-rotate fa-spin"></i> Checking...'; }
    sendMessageToDotNet({ type: 'getCacheSize' });
}

function deleteCacheData() {
    if (!confirm('Delete the entire LocalAvatarData folder? VRChat will re-download avatar files as needed.')) return;
    const btn = document.getElementById('btn-cache-delete');
    if (btn) { btn.disabled = true; btn.innerHTML = '<i class="fa-solid fa-rotate fa-spin"></i> Deleting...'; }
    sendMessageToDotNet({ type: 'deleteCache' });
}

function _handleCacheSizeResult(data) {
    const btn = document.getElementById('btn-cache-check');
    if (btn) { btn.disabled = false; btn.innerHTML = '<i class="fa-solid fa-magnifying-glass"></i> Check Size'; }
    const pathEl = document.getElementById('cache-path-text');
    const sizeEl = document.getElementById('cache-size-label');
    const result = document.getElementById('cache-action-result');
    if (pathEl) pathEl.textContent = data.path || 'Unknown';
    if (sizeEl) sizeEl.textContent = data.exists ? (data.sizeMB + ' MB') : 'Folder not found';
    if (result) result.style.display = 'none';
}

function _handleCacheDeleteResult(data) {
    const btn = document.getElementById('btn-cache-delete');
    if (btn) { btn.disabled = false; btn.innerHTML = '<i class="fa-solid fa-trash"></i> Delete Cache'; }
    const resultEl = document.getElementById('cache-action-result');
    if (resultEl) {
        resultEl.style.display = 'block';
        resultEl.style.background = data.success
            ? 'color-mix(in srgb, #27ae60 15%, var(--panel-bg-color))'
            : 'color-mix(in srgb, #c0392b 12%, var(--panel-bg-color))';
        resultEl.style.border = '1px solid ' + (data.success ? '#27ae60' : '#c0392b');
        resultEl.style.color  = 'var(--text-color)';
        resultEl.textContent  = data.message;
    }
    if (data.success) {
        const sizeEl = document.getElementById('cache-size-label');
        if (sizeEl) sizeEl.textContent = '0 MB';
        showToast('Cache deleted successfully.', 'success', 'Cache', 4000);
    } else {
        showToast('Cache deletion failed: ' + data.message, 'error', 'Cache', 5000);
    }
}

// ─── Update Check ─────────────────────────────────────────────────────────────

function requestUpdateCheck() {
    if (_updateCheckInProgress) return;
    _updateCheckInProgress = true;
    _updateExtrasUpdateUI('checking');
    sendMessageToDotNet({ type: 'checkForUpdates' });
}

function _handleUpdateCheckResult(data) {
    _updateCheckInProgress = false;
    if (data.status === 'NewVersionAvailable') {
        _pendingUpdateVersion = data.remoteVersion;
        _updateExtrasUpdateUI('available', data);
        if (!_isVersionSkipped(data.remoteVersion)) {
            showUpdateModal(data.currentVersion, data.remoteVersion);
        }
    } else if (data.status === 'UpToDate') {
        _updateExtrasUpdateUI('uptodate', data);
    } else if (data.status === 'LocalIsNewer') {
        _updateExtrasUpdateUI('localnewer', data);
        showPreReleaseModal(data.currentVersion, data.remoteVersion);
    } else {
        _updateExtrasUpdateUI('failed', data);
    }
}

function _isVersionSkipped(version) {
    try {
        const skip = JSON.parse(localStorage.getItem('updateSkip') || 'null');
        if (!skip) return false;
        return skip.version === version && Date.now() < skip.until;
    } catch { return false; }
}

function _updateExtrasUpdateUI(state, data) {
    const btn         = document.getElementById('btn-check-updates');
    const icon        = document.getElementById('btn-check-updates-icon');
    const statusText  = document.getElementById('upd-status-text');
    const progressWrap= document.getElementById('upd-progress-wrap');
    const resultPanel = document.getElementById('upd-result-panel');
    const resultBadge = document.getElementById('upd-result-badge');
    const resultIcon  = document.getElementById('upd-result-icon');
    const resultText  = document.getElementById('upd-result-text');
    const actionBtns  = document.getElementById('upd-action-buttons');

    if (state === 'checking') {
        if (btn)  { btn.disabled = true; }
        if (icon) { icon.className = 'fa-solid fa-rotate fa-spin'; }
        if (statusText)   statusText.textContent = 'Checking for updates\u2026';
        if (progressWrap) progressWrap.style.display = 'block';
        if (resultPanel)  resultPanel.style.display  = 'none';
        return;
    }

    if (btn)  { btn.disabled = false; }
    if (icon) { icon.className = 'fa-solid fa-rotate'; }
    if (progressWrap) progressWrap.style.display = 'none';
    if (resultPanel)  resultPanel.style.display  = 'block';
    if (actionBtns)   actionBtns.style.display   = 'none';

    if (state === 'uptodate') {
        if (statusText) statusText.textContent = 'Last checked: ' + new Date().toLocaleTimeString();
        if (resultBadge) { resultBadge.style.borderColor = '#2ECC71'; resultBadge.style.background = 'color-mix(in srgb, #2ECC71 8%, var(--input-bg-color))'; }
        if (resultIcon)  { resultIcon.className = 'fa-solid fa-circle-check'; resultIcon.style.color = '#2ECC71'; }
        if (resultText)    resultText.textContent = "You're up to date!";
    } else if (state === 'available') {
        if (statusText) statusText.textContent = 'Update available \u2014 v' + (data?.remoteVersion || '?');
        if (resultBadge) { resultBadge.style.borderColor = '#f0c040'; resultBadge.style.background = 'color-mix(in srgb, #f0c040 8%, var(--input-bg-color))'; }
        if (resultIcon)  { resultIcon.className = 'fa-solid fa-circle-arrow-up'; resultIcon.style.color = '#f0c040'; }
        if (resultText)    resultText.textContent = 'v' + (data?.currentVersion || '?') + ' \u2192 v' + (data?.remoteVersion || '?');
        if (actionBtns)    actionBtns.style.display = 'flex';
    } else if (state === 'localnewer') {
        if (statusText) statusText.textContent = 'Running a pre-release build';
        if (resultBadge) { resultBadge.style.borderColor = '#DE3BFF'; resultBadge.style.background = 'color-mix(in srgb, #DE3BFF 8%, var(--input-bg-color))'; }
        if (resultIcon)  { resultIcon.className = 'fa-solid fa-flask'; resultIcon.style.color = '#DE3BFF'; }
        if (resultText)    resultText.textContent = 'Running a pre-release version';
    } else {
        if (statusText) statusText.textContent = 'Failed to check for updates';
        if (resultBadge) { resultBadge.style.borderColor = '#e74c3c'; resultBadge.style.background = 'color-mix(in srgb, #e74c3c 8%, var(--input-bg-color))'; }
        if (resultIcon)  { resultIcon.className = 'fa-solid fa-circle-xmark'; resultIcon.style.color = '#e74c3c'; }
        if (resultText)    resultText.textContent = 'Failed to check \u2014 check your internet connection';
    }
}

function showUpdateModal(currentVer, remoteVer) {
    const modal      = document.getElementById('update-modal');
    const versionsEl = document.getElementById('update-modal-versions');
    if (versionsEl) versionsEl.textContent = 'v' + currentVer + ' \u2192 v' + remoteVer;
    if (modal) modal.style.display = 'flex';
}

function closeUpdateModal() {
    const modal = document.getElementById('update-modal');
    if (modal) modal.style.display = 'none';
}

function showPreReleaseModal(currentVer, remoteVer) {
    const modal      = document.getElementById('prerelease-modal');
    const versionsEl = document.getElementById('prerelease-modal-versions');
    if (versionsEl) versionsEl.textContent = 'v' + currentVer + ' → published v' + (remoteVer || '?');
    if (modal) modal.style.display = 'flex';
}

function closePreReleaseModal() {
    const modal = document.getElementById('prerelease-modal');
    if (modal) modal.style.display = 'none';
}

function openReleasesPage() {
    sendMessageToDotNet({ type: 'openUrl', url: 'https://github.com/scrim-dev/AvatarLockpick/releases' });
}

function downloadUpdate() {
    closeUpdateModal();
    sendMessageToDotNet({ type: 'downloadUpdate' });
}

function skipVersionOneMonth() {
    if (!_pendingUpdateVersion) return;
    const until = Date.now() + (30 * 24 * 60 * 60 * 1000);
    localStorage.setItem('updateSkip', JSON.stringify({ version: _pendingUpdateVersion, until }));
    closeUpdateModal();
    showToast('v' + _pendingUpdateVersion + ' will be skipped for 1 month.', 'info', 'Updates', 5000);
    const expiry = new Date(until).toLocaleDateString();
    const resultBadge = document.getElementById('upd-result-badge');
    const resultIcon  = document.getElementById('upd-result-icon');
    const resultText  = document.getElementById('upd-result-text');
    const actionBtns  = document.getElementById('upd-action-buttons');
    const statusText  = document.getElementById('upd-status-text');
    if (statusText)  statusText.textContent = 'v' + _pendingUpdateVersion + ' skipped until ' + expiry;
    if (resultBadge) { resultBadge.style.borderColor = 'var(--border-color)'; resultBadge.style.background = 'var(--input-bg-color)'; }
    if (resultIcon)  { resultIcon.className = 'fa-regular fa-moon'; resultIcon.style.color = 'var(--text-secondary-color, #b0b0b0)'; }
    if (resultText)  resultText.textContent = 'v' + _pendingUpdateVersion + ' skipped until ' + expiry;
    if (actionBtns)  actionBtns.style.display = 'none';
}

// ─── Dev Mode ─────────────────────────────────────────────────────────────────

function _initDevMode(appInfoData) {
    const navItem = document.getElementById('devmode-nav-item');
    if (navItem) navItem.style.display = '';
    const infoDiv = document.getElementById('devmode-app-info');
    if (infoDiv) {
        infoDiv.innerHTML =
            '<div>Version: <strong>' + (appInfoData.version || '?') + '</strong></div>' +
            '<div>Dev Mode: <strong style="color:var(--accent-color);">Active</strong></div>' +
            '<div>User Agent: <strong>' + navigator.userAgent.substring(0, 80) + '</strong></div>' +
            '<div>Timestamp: <strong>' + new Date().toISOString() + '</strong></div>' +
            '<div>Update Skip: <strong>' + (localStorage.getItem('updateSkip') || 'none') + '</strong></div>';
    }
}

function devTestUpdateModal() {
    _pendingUpdateVersion = '99.9';
    showUpdateModal('3.0', '99.9');
}

function devTestActionPopup() {
    if (popupTitle)   popupTitle.textContent = 'Dev Mode Test';
    if (popupMessage) popupMessage.innerHTML = '<i class="fa-solid fa-flask" style="color:var(--accent-color);margin-right:8px;"></i> This is a test of the action popup from Dev Mode.';
    if (actionPopup)  actionPopup.classList.add('show');
}

function devTestDownloadProgress() {
    showDownloadProgress('Test Download (Dev Mode)');
    let p = 0;
    const interval = setInterval(() => {
        p += 6;
        updateDownloadProgress(p, p + '% complete', 'Test Download (Dev Mode)');
        if (p >= 100) { clearInterval(interval); setTimeout(closeDownloadProgress, 600); }
    }, 80);
}

function devTestServiceModal() {
    const modal = document.getElementById('service-close-modal');
    if (modal) modal.style.display = 'flex';
}

function devTestToast(type) {
    const msgs = { success: 'Success toast test', error: 'Error toast test', warn: 'Warning toast test', info: 'Info toast test' };
    const prev = toastNotificationsEnabled;
    toastNotificationsEnabled = true;
    showToast(msgs[type] || 'Toast test', type, 'Dev Mode', 3500);
    toastNotificationsEnabled = prev;
}

function devSimulateUpdateAvailable() {
    _handleUpdateCheckResult({ status: 'NewVersionAvailable', currentVersion: '3.0', remoteVersion: '99.9' });
}

function devSimulateUpToDate() {
    _handleUpdateCheckResult({ status: 'UpToDate', currentVersion: '3.0', remoteVersion: '3.0' });
}

function devSimulateLocalNewer() {
    _handleUpdateCheckResult({ status: 'LocalIsNewer', currentVersion: '3.0', remoteVersion: '2.0' });
}

function devSimulateFailed() {
    _handleUpdateCheckResult({ status: 'FailedToCheck', currentVersion: '3.0' });
}

function devClearUpdateSkip() {
    localStorage.removeItem('updateSkip');
    showToast('Update skip cleared.', 'success', 'Dev Mode', 3000);
    const infoDiv = document.getElementById('devmode-app-info');
    if (infoDiv) {
        const rows = infoDiv.querySelectorAll('div');
        rows.forEach(r => { if (r.textContent.startsWith('Update Skip:')) r.querySelector('strong').textContent = 'none'; });
    }
}