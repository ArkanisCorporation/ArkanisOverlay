/**
 * @type {import('semantic-release').GlobalConfig}
 */
export default {
    branches: [
        'release/+([0-9])?(.{+([0-9]),x}).x',
        {
            name: "release/stable",
            channel: "stable"
        },
        {
            name: "release/rc",
            channel: "rc",
            prerelease: "rc"
        },
        {
            name: "main",
            channel: "staging",
            prerelease: "dev"
        },
        {
            name: "ci",
            channel: "ci",
            prerelease: "do-not-use"
        },
    ],
    repositoryUrl: "ArkanisCorporation/ArkanisBackend",
    tagFormat: "v${version}",
    debug: false,
    plugins: [
        "@semantic-release/commit-analyzer",
        "@semantic-release/release-notes-generator",
        "@semantic-release/github",
    ],
};
