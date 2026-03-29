/**
 * AvatarLockpick GUI - Consolidated JavaScript
 */

// --- Elements Cache ---
let splashScreen, appWrapper, tabButtons, tabPanels, animationsToggle, userIdInput, censorUserIdToggle;
let avatarIdInput, responseSpan, colorPickers, htmlElement, actionPopup, popupTitle, popupMessage;
let unlockNoticePopup, noticeTimerSpan, sidebarToggle, loadedAvatarInfoPanel, loadedAvatarIdSpan;
let loadedUserIdSpan, unlockActionButtons, unlockWarningPanel, confirmUnloadPopup, presetButtons;
let historyListContainer, noHistoryMessage, importJsonPopup, importJsonTextarea, importErrorMessage;
let netResponsePopup, netResponseContent, notePopup, noteTextarea, appLogsContainer, backendMessagesToggle;

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
let showBackendMessages = false; // Toggle for showing backend communication popups

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
    if (themeSettings.hasOwnProperty('--accent-color')){
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

    const sidebarExpanded = localStorage.getItem('sidebarExpanded') === 'true';
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

    updateUnlockUI();
}

function updateSidebarToggleIcon(isExpanded) {
    const icon = sidebarToggle.querySelector('i');
    if (icon) {
        icon.className = isExpanded ? 'fa-solid fa-arrow-left' : 'fa-solid fa-bars';
    }
}

// --- Button Click Functions ---
function callDotNetSendIDs() {
    const avatarId = avatarIdInput.value;
    const userId = userIdInput.value;

    const message = { type: 'avatarInfo', avatarId, userId };
    sendMessageToDotNet(message);
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

    isAvatarLoaded = true;
    window.loadedAvatarId = avatarId;
    window.loadedUserId = userId;

    updateUnlockUI();
    addAvatarToHistory(avatarId, userId);
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
    sendMessageToDotNet(message);

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
                <h4>${item.avatarId}</h4>
                <p><strong>User:</strong> ${item.userId || 'N/A'}</p>
                <p><small>Loaded: ${formattedDate}</small></p>
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
    const existingIndex = avatarHistory.findIndex(item => item.avatarId === avatarId);
    if (existingIndex > -1) {
        existingNote = avatarHistory[existingIndex].note;
        avatarHistory.splice(existingIndex, 1);
    }

    avatarHistory.unshift({
        avatarId: avatarId,
        userId: userId,
        timestamp: new Date().getTime(),
        note: existingNote
    });

    if (avatarHistory.length > 50) {
        avatarHistory.pop();
    }

    localStorage.setItem('avatarHistory', JSON.stringify(avatarHistory));
    renderHistory();
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

// --- Logs Tab Functions ---
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
    if (isAvatarLoaded) {
        loadedAvatarInfoPanel.style.display = 'block';
        unlockWarningPanel.style.display = 'none';
        unlockActionButtons.forEach(btn => btn.disabled = false);
    } else {
        loadedAvatarInfoPanel.style.display = 'none';
        unlockWarningPanel.style.display = 'block';
        unlockActionButtons.forEach(btn => btn.disabled = true);
    }
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
document.addEventListener('DOMContentLoaded', function() {
    // Cache all DOM elements
    splashScreen = document.getElementById('splash-screen');
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
});

// --- Window Load Event ---
window.addEventListener('load', () => {
    initialize();
    initCustomDbSettings();

    setTimeout(() => {
        splashScreen.classList.add('fade-out');
    }, 1500);

    splashScreen.addEventListener('transitionend', () => {
        splashScreen.style.display = 'none';
        appWrapper.style.display = 'flex';
    }, { once: true });

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
        console.log(`Show backend messages: ${showBackendMessages}`);
    });

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

            if(netResponseContent) {
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