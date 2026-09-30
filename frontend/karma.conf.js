const fs = require('fs');
const path = require('path');

// Auto-detect Chrome from puppeteer cache or system paths
if (!process.env.CHROME_BIN) {
  const puppeteerBase = path.join(process.env.USERPROFILE || process.env.HOME || '', '.cache', 'puppeteer', 'chrome');
  if (fs.existsSync(puppeteerBase)) {
    const versions = fs.readdirSync(puppeteerBase);
    for (const v of versions) {
      const candidate = path.join(puppeteerBase, v, 'chrome-win64', 'chrome.exe');
      if (fs.existsSync(candidate)) {
        process.env.CHROME_BIN = candidate;
        break;
      }
    }
  }
}

module.exports = function (config) {
  config.set({
    basePath: '',
    frameworks: ['jasmine', '@angular-devkit/build-angular'],
    plugins: [
      require('karma-jasmine'),
      require('karma-chrome-launcher'),
      require('karma-jasmine-html-reporter'),
      require('karma-coverage'),
      require('@angular-devkit/build-angular/plugins/karma')
    ],
    client: {
      jasmine: {},
      clearContext: false
    },
    jasmineHtmlReporter: {
      suppressAll: true
    },
    coverageReporter: {
      dir: require('path').join(__dirname, './coverage/order-flow-app'),
      subdir: '.',
      reporters: [
        { type: 'html' },
        { type: 'text-summary' }
      ]
    },
    reporters: ['progress', 'kjhtml'],
    browsers: ['ChromeHeadlessNoSandbox'],
    customLaunchers: {
      ChromeHeadlessNoSandbox: {
        base: 'ChromeHeadless',
        flags: ['--no-sandbox', '--disable-gpu', '--disable-dev-shm-usage']
      }
    },
    restartOnFileChange: true
  });
};
