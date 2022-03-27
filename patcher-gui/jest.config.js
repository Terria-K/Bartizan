const FormData = require('form-data');
const fetch = require('node-fetch');

const transformIgnoreModules = [];

module.exports = {
  preset: 'ts-jest',
  setupFiles: [],
  setupFilesAfterEnv: [],
  testRegex: '.spec.[tj]sx?$',
  testPathIgnorePatterns: ['dist/'],
  transformIgnorePatterns: [
    `node_modules/(?!(${transformIgnoreModules.join('|')})/)`,
  ],
  clearMocks: true,
  globals: {
    env: 'test',
  },
  collectCoverage: false,
};
