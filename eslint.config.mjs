// eslint.config.mjs
export default [
  {
    files: ["**/*.js"],
    languageOptions: {
      ecmaVersion: 2021,
      sourceType: "module"
    },
    rules: {
      "indent": ["error", 4, { "SwitchCase": 1 }],
      "linebreak-style": ["error", "windows"],
      "quotes": ["error", "double"],
      "semi": ["error", "always"]
    }
  }
];


 
