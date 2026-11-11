#!/usr/bin/env node
// Guard-rail hook. Claude Code runs this before every file or shell tool call (see hooks.json)
// and hands it the call as JSON on standard input. Exit code 2 blocks the call, and the text
// on standard error is shown to the agent as the reason. Exit code 0 lets the call through.

const BLOCKED = [
  [/(^|[\\/\s"'])\.env(\.[\w-]+)?($|[\s"'])/i, "an environment file"],
  [/(^|[\\/\s"'])secrets?[\\/]/i, "the secrets folder"],
  [/\.(pfx|p12|pem|key)($|[\s"'])/i, "a certificate or private key"],
  [/appsettings\.Production\.json/i, "production settings"],
  [/(^|[\\/\s"'])id_(rsa|ed25519)($|[\s"'.])/i, "an SSH key"],
];

let input = "";
process.stdin.setEncoding("utf8");
process.stdin.on("data", (chunk) => (input += chunk));
process.stdin.on("end", () => {
  let call;
  try {
    call = JSON.parse(input);
  } catch {
    // A guard that cannot read its input says no.
    block("the guard-rail hook could not read the tool call");
    return;
  }

  // Check every text value the tool was given: a file path, a search pattern, a shell command.
  const values = Object.values(call.tool_input ?? {}).filter((value) => typeof value === "string");

  for (const value of values) {
    for (const [pattern, what] of BLOCKED) {
      if (pattern.test(value)) {
        block(`${call.tool_name} would touch ${what}`);
        return;
      }
    }
  }

  process.exit(0);
});

function block(reason) {
  process.stderr.write(
    `Blocked by the company guard rail: ${reason}. ` +
      "Secrets and production settings stay out of AI sessions. Ask a person if you need the value.\n"
  );
  process.exit(2);
}
