/* i18n_loader.js — shared translation loader for sub-windows */

(function () {
    const TRANSLATIONS_URL = '../translations.json';
    let _data = null;
    let _lang = 'en';
    let _ready = false;
    let _pendingLang = null;

    function _apply(lang) {
        if (!_data) return;
        if (!_data[lang]) lang = 'en';
        _lang = lang;
        const dict = _data[lang] || {};

        // data-i18n: set innerHTML from key
        document.querySelectorAll('[data-i18n]').forEach(el => {
            const key = el.getAttribute('data-i18n');
            if (key && dict[key] !== undefined) el.innerHTML = dict[key];
        });

        // data-i18n-ph: set placeholder from key
        document.querySelectorAll('[data-i18n-ph]').forEach(el => {
            const key = el.getAttribute('data-i18n-ph');
            if (key && dict[key] !== undefined) el.placeholder = dict[key];
        });

        // data-i18n-title: set title attribute from key
        document.querySelectorAll('[data-i18n-title]').forEach(el => {
            const key = el.getAttribute('data-i18n-title');
            if (key && dict[key] !== undefined) el.title = dict[key];
        });
    }

    function load() {
        const saved = localStorage.getItem('appLanguage') || 'en';
        return fetch(TRANSLATIONS_URL)
            .then(r => r.json())
            .then(data => {
                _data = data;
                _ready = true;
                const lang = _pendingLang || saved;
                _pendingLang = null;
                _apply(lang);
            })
            .catch(err => {
                console.warn('[i18n] Failed to load translations:', err);
                _ready = true;
            });
    }

    window.i18n = {
        /** Translate a key, fallback to 'en', then the key itself */
        t(key) {
            if (!_data) return key;
            const d = _data[_lang] || _data['en'] || {};
            return d[key] !== undefined ? d[key] : ((_data['en'] || {})[key] ?? key);
        },
        setLang(lang) {
            if (!_ready) { _pendingLang = lang; return; }
            localStorage.setItem('appLanguage', lang);
            _apply(lang);
        },
        getLang() { return _lang; },
        isReady() { return _ready; }
    };

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', load);
    } else {
        load();
    }
})();
