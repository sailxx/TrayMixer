// SVG-плашки README в стиле Okto Calc: чёрный фон, свечения и синий акцент, шрифты Golos Text и JetBrains Mono
// (урезаются под каждый язык). Тексты — strings.json. Скриншот окна собирает tools\readme-art.cmd.
// Шрифты (OFL-1.1) положить в tools/readme/.font/: golos.ttf (Golos Text) и jbmono.ttf (JetBrains Mono).
// Запуск: cd tools/readme && npm install && node build.mjs
import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
import { createKit, ICONS } from './kit.mjs';

const DIR = path.dirname(fileURLToPath(import.meta.url));
const REPO = path.resolve(DIR, '../..');
const OUT = path.join(REPO, 'assets/readme');
const STR = JSON.parse(fs.readFileSync(path.join(DIR, 'strings.json'), 'utf8'));
const VERSION = fs.readFileSync(path.join(REPO, 'TrayMixer.cs'), 'utf8').match(/AssemblyVersion\("(\d+\.\d+)/)[1];

const B = '#4CC2FF'; // акцент Windows 11 (тёмная тема)
const K = createKit({
  golos: path.join(DIR, '.font/golos.ttf'),
  mono: path.join(DIR, '.font/jbmono.ttf'),
  accent: { hi: B, grad: ['#A8E1FF', '#3B8BFF'], on: '#001A2E', glow: '#2E8BFF', glow2: '#7A5CFF', btn: ['#6CCFFF', '#2C9BE8'] },
});
const { P, T, card, chip, sectionHead, iconBox, lines, wrap, one, fitSize, fitText, delay, svg } = K;

// Логотип — геометрия значка программы: три ползунка
function mixer(x, y, n, level = B) {
  let s = '';
  const th = n * 0.07, kr = n * 0.11, x0 = x + n * 0.08, x1 = x + n * 0.92;
  [[0.22, 0.72], [0.5, 0.3], [0.78, 0.55]].forEach(([row, lv]) => {
    const cy = y + n * row, kx = x0 + kr + (x1 - x0 - 2 * kr) * lv;
    s += `<rect x="${x0}" y="${cy - th / 2}" width="${x1 - x0}" height="${th}" rx="${th / 2}" fill="#fff" fill-opacity=".35"/>` +
      `<rect x="${x0}" y="${cy - th / 2}" width="${kx - x0}" height="${th}" rx="${th / 2}" fill="${level}"/>` +
      `<circle cx="${kx}" cy="${cy}" r="${kr}" fill="#fff"/><circle cx="${kx}" cy="${cy}" r="${kr * 0.45}" fill="${level}"/>`;
  });
  return s;
}

// Окно-флайаут Windows 11 с ползунками (как в программе)
const ROW_ICONS = [
  '<path d="M4 16v-3a8 8 0 0 1 16 0v3m-16 0v5h4v-7H4m16 2v5h-4v-7h4"/>',
  '<rect x="6" y="3" width="12" height="18" rx="2.5"/><circle cx="12" cy="14" r="3.2"/><circle cx="12" cy="7.5" r="1"/>',
  '<rect x="2.5" y="4" width="19" height="13" rx="2"/><path d="M9 21h6M12 17v4"/>',
  '<circle cx="12" cy="12" r="8.5"/><path d="M3.5 12h17M12 3.5c2.6 2.4 3.8 5.2 3.8 8.5s-1.2 6.1-3.8 8.5c-2.6-2.4-3.8-5.2-3.8-8.5s1.2-6.1 3.8-8.5z"/>',
  '<path d="M7 9.5h10M7 14.5h6"/><rect x="3" y="4" width="18" height="16" rx="3"/>',
];
function flyout(x, y) {
  const w = 300, h = 300;
  let s = `<rect x="${x}" y="${y}" width="${w}" height="${h}" rx="12" fill="#202020" fill-opacity=".92" stroke="#3a3a3a"/>`;
  s += `<rect x="${x}" y="${y}" width="${w}" height="${h}" rx="12" fill="url(#g1)" opacity=".5"/>`;
  const levels = [0.72, 0.45, 0.6, 0.85, 0.3];
  levels.forEach((lv, i) => {
    const ry = y + 34 + i * 54;
    const dev = i < 3;
    s += `<g transform="translate(${x + 18} ${ry - 12})" fill="none" stroke="${i === 0 ? B : '#E6E6E6'}" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round">${ROW_ICONS[i]}</g>`;
    s += `<rect x="${x + 54}" y="${ry - 9}" width="${[96, 74, 84, 60, 70][i]}" height="6" rx="3" fill="#E6E6E6" fill-opacity="${dev ? 0.85 : 0.55}"/>`;
    const sx = x + 54, sw = 196, kx = sx + sw * lv;
    s += `<rect x="${sx}" y="${ry + 8}" width="${sw}" height="4" rx="2" fill="#fff" fill-opacity=".22"/><rect x="${sx}" y="${ry + 8}" width="${kx - sx}" height="4" rx="2" fill="${B}"/>`;
    s += `<circle cx="${kx}" cy="${ry + 10}" r="9" fill="#454545"/><circle cx="${kx}" cy="${ry + 10}" r="${i === 0 ? 5.5 : 4.5}" fill="${B}"${i === 0 ? ' class="pulse2"' : ''}/>`;
    s += `<text class="m" x="${x + w - 18}" y="${ry - 3}" text-anchor="end" font-size="11" font-weight="600" fill="#9A9A9A">${T(String(Math.round(lv * 100)))}</text>`;
    if (i === 2) s += `<line x1="${x + 18}" y1="${ry + 30}" x2="${x + w - 18}" y2="${ry + 30}" stroke="#fff" stroke-opacity=".08"/>`;
  });
  return s;
}
const PULSE = '.pulse2{animation:k 2.4s ease-in-out infinite;transform-box:fill-box;transform-origin:center}@keyframes k{50%{transform:scale(1.25)}}@media (prefers-reduced-motion:reduce){.pulse2{animation:none}}';

// ---------- HERO ----------
function hero(l) {
  const W = 880;
  let b = `<g class="r"><rect x="40" y="36" width="36" height="36" rx="10" fill="#0E0E0E" stroke="#3a3a3a"/>${mixer(42, 38, 32)}
<text x="88" y="62" font-size="24" font-weight="800" letter-spacing="-.4">${T('TrayMixer')}</text></g>`;
  b += `<g class="r" ${delay(1)}>${chip(232, 39, 'v' + VERSION).s}</g>`;
  const ts = 34;
  const t1 = wrap(l.line1, 470, ts, 800), t2 = wrap('*' + l.line2 + '*', 470, ts, 800);
  const all = [...t1, ...t2];
  b += `<g class="r" ${delay(2)}>${lines(40, 140, all, { size: ts, weight: 800, lh: ts * 1.22, spacing: '-.8' })}</g>`;
  const y = 140 + (all.length - 1) * ts * 1.22;
  const H = Math.max(y + 70, 380);
  b += `<g class="r" ${delay(3)}><ellipse cx="700" cy="${H / 2}" rx="190" ry="180" fill="url(#g1)"/>${flyout(550, (H - 300) / 2)}</g>`;
  return svg(W, H, l.alt, b + `<style>${PULSE}</style>`);
}

// ---------- ВОЗМОЖНОСТИ ----------
const FEAT_ICONS = [ROW_ICONS[0], '<rect x="3" y="3" width="18" height="18" rx="4"/><path d="M8 12h8M12 8v8"/>', '<rect x="2" y="4" width="20" height="16" rx="3"/><path d="M2 9h20"/>',
  '<path d="M3 12s3-6 9-6 9 6 9 6-3 6-9 6-9-6-9-6z"/><path d="M4 4l16 16"/>', '<path d="M13 2l-8 12h7l-1 8 8-12h-7z"/>', ICONS.shield];
function features(l) {
  const head = sectionHead(l.featuresTitle, l.featuresHead);
  let b = head.s;
  const cw = (800 - 2 * 12) / 3;
  const items = l.features.map(([t, d]) => [t, wrap(d, cw - 40, 14, 500)]);
  const ch = 92 + Math.max(...items.map(([, d]) => d.length)) * 20 + 4;
  items.forEach(([t, d], i) => {
    const x = 40 + (i % 3) * (cw + 12), y = head.y + Math.floor(i / 3) * (ch + 12);
    b += `<g class="r" ${delay(2 + i * 0.6)}>${card(x, y, cw, ch)}${iconBox(x + 20, y + 20, FEAT_ICONS[i])}
${fitText(x + 72, y + 46, t, cw - 88, { size: 17, weight: 700 })}
${lines(x + 20, y + 92, d, { size: 14, weight: 500, lh: 20, fill: P.muted })}</g>`;
  });
  return svg(880, head.y + 2 * ch + 12 + 36, l.featuresAlt, b);
}

// ---------- В ЦИФРАХ ----------
function numbers(l) {
  const head = sectionHead(l.numbersChip, l.numbersHead);
  let b = head.s;
  const w = (800 - 3 * 12) / 4;
  l.numbers.forEach(([n, d], i) => {
    const x = 40 + i * (w + 12), y = head.y;
    const [num, unit = ''] = n.split(' ');
    b += `<g class="r" ${delay(2 + i)}>${card(x, y, w, 120)}
<text class="m" x="${x + 22}" y="${y + 58}" font-size="40" font-weight="700" letter-spacing="-1.5" fill="url(#tg)">${T(num)}<tspan font-size="17" fill="${P.muted}">${T(unit ? ' ' + unit : '')}</tspan></text>
${fitText(x + 22, y + 92, d, w - 40, { size: 14, weight: 500, fill: P.muted })}</g>`;
  });
  return svg(880, head.y + 120 + 36, l.numbersAlt, b);
}

// ---------- БЕЗОПАСНОСТЬ ----------
function security(l) {
  const head = sectionHead(l.securityTitle, l.securityHead);
  let b = head.s;
  const cw = (800 - 12) / 2;
  const rows = l.security.map((s) => wrap(s, cw - 72, 14, 500));
  const rh = Math.max(...rows.map((r) => r.length)) * 20 + 30;
  rows.forEach((r, i) => {
    const x = 40 + (i % 2) * (cw + 12), y = head.y + Math.floor(i / 2) * (rh + 10);
    b += `<g class="r" ${delay(2 + i * 0.5)}>${card(x, y, cw, rh, 14)}
<circle cx="${x + 28}" cy="${y + rh / 2}" r="11" fill="${B}" fill-opacity=".14" stroke="${B}" stroke-opacity=".3"/><path d="M${x + 23} ${y + rh / 2}l3.4 3.4 6-6.4" fill="none" stroke="${B}" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/>
${lines(x + 52, y + rh / 2 + 5 - (r.length - 1) * 10, r, { size: 14, weight: 500, lh: 20, fill: '#D6D6D6' })}</g>`;
  });
  return svg(880, head.y + 3 * rh + 20 + 36, l.securityAlt, b);
}

// ---------- сборка ----------
const langs = {};
for (const [lang, l] of Object.entries(STR)) {
  langs[lang] = () => ({
    [`hero-${lang}.svg`]: hero(l),
    [`cta-${lang}.svg`]: K.button(l.cta, ICONS.download, true),
    [`features-${lang}.svg`]: features(l),
    [`numbers-${lang}.svg`]: numbers(l),
    [`security-${lang}.svg`]: security(l),
  });
}
await K.build(OUT, langs, 'TrayMixerv.');
