// Narrow standalone cards wrap naturally in GitHub README without custom CSS.
const fs = require('node:fs');
const path = require('node:path');
const root = path.resolve(__dirname, '../..');
const reference = fs.readFileSync(path.join(__dirname, 'reference/features.svg'), 'utf8');
const style = reference.match(/<style>[\s\S]*?<\/style>/)[0];
const defs = reference.match(/<defs>[\s\S]*?<\/defs>/)[0]
  .replaceAll('#1478F0', '#229BDC').replaceAll('#7CC4FF', '#A8E1FF')
  .replace('width="880" height="596"', 'width="320" height="320"');
const escape = s => s.replaceAll('&', '&amp;').replaceAll('<', '&lt;').replaceAll('"', '&quot;');
const text = (x, y, s, size = 17, color = '#B8B8B8', mono = false) =>
  `<text x="${x}" y="${y}" font-size="${size}" font-weight="${size >= 20 ? 700 : 500}" fill="${color}"${mono ? ' class="m"' : ''}>${escape(s)}</text>`;
const groups = {
  benefit: [
    ['Your audio, separately', 'Control each output and app.', 'Turn a game down without', 'losing your call.'],
    ['Windows display tools', 'Brightness, blue-light filtering', 'and software dimming', 'alongside your audio.'],
    ['Built for the desktop', 'Tray access, wheel adjustments', 'and mute. Hide the rows', 'you do not need.'],
    ['A look that fits', 'Rounded Windows flyout.', 'Linux themes follow GNOME', 'or your chosen palette.'],
    ['Local by design', 'No account, ads or analytics.', 'Your preferences stay', 'on your computer.'],
    ['Open and inspectable', 'Apache 2.0 source and', 'build instructions. Read each', 'platform release’s changes.']
  ],
  roadmap: [
    ['Available now', 'Windows 3.0 stable', 'Device and application volume', 'Brightness and blue-light tools', '', 'Linux 3.1 preview', 'Audio, GNOME colors, themes', 'DEB, RPM and AppImage'],
    ['Validate next', 'GNOME desktop', 'Tray, themes and real hardware', '', 'Fedora / Debian', 'Installation on clean systems', '', 'Priorities may change.'],
    ['Explore later', 'Linux display controls', 'Desktop-specific APIs needed.', '', 'Ongoing refinements', 'Guided by bugs and feedback.', '', 'No fixed delivery dates.']
  ],
  trust: [
    ['Local settings', 'No account, ads or analytics.', 'Preferences stay', 'on your computer.'],
    ['Public source', 'Read the code and', 'build instructions. Apache 2.0.', 'No security audit claimed.'],
    ['Verify downloads', 'Releases are unsigned.', 'Check the release source and', 'compare published checksums.']
  ]
};
for (const [group, cards] of Object.entries(groups)) {
  cards.forEach(([title, ...lines], i) => {
    const height = group === 'roadmap' ? 320 : 198;
    let body = `<g clip-path="url(#clip)"><rect width="320" height="${height}" rx="20" fill="url(#bg)"/><circle cx="290" cy="0" r="210" fill="url(#g1)"/><rect x=".5" y=".5" width="319" height="${height - 1}" rx="20" fill="url(#cg)" fill-opacity=".65" stroke="#333"/><rect x="20" y="1" width="280" height="1" fill="url(#hl)"/></g>`;
    if (group === 'benefit') {
      body += `<circle cx="32" cy="32" r="15" fill="#4CC2FF"/>` + text(27, 38, String(i + 1), 17, '#021226', true);
    } else {
      body += text(20, 38, group === 'roadmap' ? ['SHIPPED', 'NEXT', 'LATER'][i] : 'TRUST', 17, '#74D3FF', true);
    }
    body += text(20, 78, title, 22, '#FFFFFF');
    lines.forEach((line, j) => { body += text(20, 112 + j * 26, line); });
    const label = `${title}: ${lines.filter(Boolean).join(' ')}`;
    fs.writeFileSync(path.join(root, 'assets/readme', `${group}-${i + 1}.svg`), `<svg xmlns="http://www.w3.org/2000/svg" width="320" height="${height}" viewBox="0 0 320 ${height}" role="img" aria-label="${escape(label)}">${style}${defs}${body}</svg>\n`);
  });
}
console.log('PASS: 12 wrapping README cards generated; 320px wide, body text 17px');
