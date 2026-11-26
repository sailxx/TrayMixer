import { ICONS } from './kit.mjs';

// Match the English Tobi README layout using TrayMixer's own screenshots and release data.
export function showcase(kit, screenshot) {
  const { A, P, T, badge, button, card, chip, delay, fitText, lines, one, sectionHead, svg, width, wrap } = kit;
  const group = (step, body) => `<g class="r" ${delay(step)}>${body}</g>`;
  const dot = (x, y, radius, color = A.hi) => `<circle cx="${x}" cy="${y}" r="${radius}" fill="${color}"/>`;
  const text = (x, y, value, opts) => fitText(x, y, value, opts.maxWidth, opts);
  const label = (x, y, value, fill) => text(x, y, value, { maxWidth: 230, size: 12, weight: 600, mono: true, spacing: 1, fill });

  function keycap(x, y, name) {
    const w = Math.max(30, Math.round(width(name, 13, 600, { mono: true }) + 22));
    return {
      w,
      svg: `<rect x="${x}" y="${y}" width="${w}" height="30" rx="8" fill="#161616" stroke="#3a3a3a"/>
<rect x="${x + 1}" y="${y + 26}" width="${w - 2}" height="3" rx="1.5" fill="#000" fill-opacity=".6"/>
<text class="m" x="${x + w / 2}" y="${y + 19.5}" text-anchor="middle" font-size="13" font-weight="600">${T(name)}</text>`,
    };
  }

  function hero() {
    const title = ['Your sound,', '*one click away.*'];
    const blurb = ['Mix outputs and apps from the system tray.', 'On Windows, keep display controls in reach.'];
    const cards = [
      ['Output devices', 'Switch speakers and headphones'],
      ['App volume', 'Turn each app up or down'],
      ['Display controls', 'Brightness and warmth on Windows'],
      ['Local preferences', 'No account or telemetry'],
    ];
    let body = group(0, chip(40, 39, 'Windows v3.0.2').s)
      + group(2, lines(40, 146, title.map(one), { size: 44, weight: 800, lh: 50, spacing: '-1' }))
      + group(3, lines(40, 236, blurb.map(one), { size: 16, lh: 24, fill: P.muted }));
    cards.forEach(([title, detail], i) => {
      const x = 40 + (i % 2) * 230;
      const y = 290 + Math.floor(i / 2) * 86;
      body += group(4 + i, card(x, y, 218, 74) + dot(x + 22, y + 27, 4)
        + text(x + 34, y + 32, title, { maxWidth: 170, size: 16, weight: 700 })
        + text(x + 20, y + 54, detail, { maxWidth: 178, size: 13, fill: P.muted }));
    });
    const tray = keycap(0, 380, 'Tray click');
    const panel = `<ellipse cx="680" cy="230" rx="190" ry="200" fill="url(#g1)"/>
${card(512, 54, 336, 300, 24)}
<g clip-path="url(#flyout)">
<svg x="548" y="74" width="264" height="260" viewBox="215 60 451 445" overflow="hidden">
<image width="880" height="564" href="data:image/png;base64,${screenshot}"/>
</svg>
</g>
<g transform="translate(${680 - tray.w / 2} 0)">${tray.svg}</g>`;
    const clip = '<clipPath id="flyout"><rect x="548" y="74" width="264" height="260" rx="8"/></clipPath>';
    return svg(880, 480, 'TrayMixer for Windows and Linux, with a Windows mixer screenshot and four highlights', body + group(3, panel), clip);
  }

  function features() {
    const head = sectionHead('Features', 'Everyday adjustments — *in one tray panel*');
    const items = [
      ['Output devices', ['Give speakers, headphones and monitors', 'their own volume and mute controls.']],
      ['Per-app volume', ['Turn a game down while keeping your call.', 'Adjust individual streams on Linux.']],
      ['Windows display controls', ['Adjust brightness, blue-light filtering', 'and software dimming from the mixer.']],
      ['Tray controls', ['Click to open, scroll to adjust and mute.', 'Hide rows and restore them from the menu.']],
      ['Desktop appearance', ['Use the Windows flyout or choose a Linux', 'theme and follow GNOME colors.']],
      ['Local preferences', ['Choose whether to start at login.', 'Settings stay on your computer.']],
    ];
    let body = head.s;
    items.forEach(([title, copy], i) => {
      const x = 40 + (i % 2) * 408;
      const y = head.y + Math.floor(i / 2) * 148;
      body += group(2 + i, card(x, y, 392, 132, 20) + badge(x + 40, y + 42, i + 1)
        + text(x + 68, y + 48, title, { maxWidth: 300, size: 20, weight: 700 })
        + lines(x + 24, y + 92, copy.map(one), { size: 15, lh: 22, fill: P.muted }));
    });
    return svg(880, head.y + 428 + 36, 'Six TrayMixer features: devices, app volume, display controls, tray controls, themes and local preferences', body);
  }

  function controls() {
    const head = sectionHead('Controls', 'Tray and mixer *shortcuts*');
    const items = [
      ['Tray click', 'Open or close the mixer'],
      ['Middle click', 'Mute or unmute the default output'],
      ['Mouse wheel', 'Adjust a slider by 2 points'],
      ['Output name', 'Set as the default output'],
      ['Right click', 'Hide a mixer row'],
      ['Esc', 'Close the Windows flyout'],
    ];
    let body = head.s;
    items.forEach(([key, action], i) => {
      const x = 40 + (i % 2) * 408;
      const y = head.y + Math.floor(i / 2) * 70;
      const cap = keycap(x + 20, y + 16, key);
      body += group(2 + i, card(x, y, 392, 62, 16) + cap.svg
        + text(x + 32 + cap.w, y + 36, action, { maxWidth: 342 - cap.w, size: 15, weight: 600, fill: i === 0 ? A.hi : P.ink }));
    });
    const y = head.y + 224;
    body += group(8, text(40, y, 'Windows shortcuts shown. Linux tray clicks depend on your desktop.', { maxWidth: 800, size: 14, fill: P.muted }));
    return svg(880, y + 34, 'Windows tray and mixer controls: click, middle click, mouse wheel, output name, right click and Escape', body);
  }

  function states() {
    const head = sectionHead('Mixer', 'Six things you can *adjust*');
    const items = [
      ['Output', '#5AB8FF', '<path d="M4 9h4l5-4v14l-5-4H4zM17 9a4 4 0 0 1 0 6m2.5-8a7 7 0 0 1 0 10"/>'],
      ['App volume', '#8B5CF6', '<rect x="3" y="5" width="18" height="14" rx="2"/><path d="M3 9h18m-9 6h5m-2.5-2v4"/>'],
      ['Brightness', '#3B9EFF', '<circle cx="12" cy="12" r="4"/><path d="M12 2v3m0 14v3M2 12h3m14 0h3M5 5l2 2m10 10 2 2m0-14-2 2M7 17l-2 2"/>'],
      ['Blue light', '#F5A524', '<path d="M21 14a9 9 0 0 1-11-11 9 9 0 1 0 11 11z"/>'],
      ['Dimming', '#34D399', '<circle cx="12" cy="12" r="9"/><path d="M12 3v18m0-18a9 9 0 0 1 0 18z" fill="currentColor" fill-opacity=".35"/>'],
      ['Mute', '#94A2B8', '<path d="M4 9h4l5-4v14l-5-4H4zM16 10l5 5m0-5-5 5"/>'],
    ];
    const w = (800 - 5 * 12) / 6;
    let body = head.s;
    items.forEach(([name, color, icon], i) => {
      const x = 40 + i * (w + 12);
      const y = head.y;
      const cx = x + w / 2;
      body += group(2 + i, card(x, y, w, 170, 18)
        + `<circle cx="${cx}" cy="${y + 78}" r="30" fill="${color}" fill-opacity=".16" stroke="${color}" stroke-opacity=".4"/>
<g transform="translate(${cx - 18} ${y + 60}) scale(1.5)" color="${color}" stroke="${color}" fill="none" stroke-width="1.6" stroke-linecap="round" stroke-linejoin="round">${icon}</g>`
        + dot(x + 18, y + 147, 4, color) + text(x + 28, y + 152, name, { maxWidth: w - 36, size: 14, weight: 700 }));
    });
    return svg(880, head.y + 204, 'Mixer controls: output volume, app volume, Windows brightness, blue light, dimming and mute', body);
  }

  function numbers() {
    const head = sectionHead('By the numbers', 'TrayMixer *at a glance*');
    const items = [
      ['219', ' KB', ['Standalone Windows', 'executable']],
      ['0', '', ['Accounts or', 'telemetry']],
      ['2', '', ['Desktop platforms']],
      ['3', '', ['Linux package', 'formats']],
      ['5', '', ['Linux appearance', 'options']],
    ];
    const w = (800 - 4 * 12) / 5;
    let body = head.s;
    items.forEach(([number, unit, copy], i) => {
      const x = 40 + i * (w + 12);
      const y = head.y;
      body += group(2 + i, card(x, y, w, 124, 18)
        + `<text class="m" x="${x + 20}" y="${y + 56}" font-size="38" font-weight="700" letter-spacing="-1" fill="url(#tg)">${T(number)}<tspan font-size="16" fill="${P.muted}">${T(unit)}</tspan></text>`
        + lines(x + 20, y + 86, copy.map(one), { size: 13, lh: 18, fill: P.muted }));
    });
    return svg(880, head.y + 158, '219 KB Windows executable, zero accounts or telemetry, two platforms, three Linux package formats and five appearance options', body);
  }

  function roadmap() {
    const head = sectionHead('Roadmap', 'What shipped — *and what comes next*');
    const releases = [
      ['3.0.0', 'Windows · Charcoal glass and offline setup', 'Nov 16'],
      ['3.1.0', 'Linux · First preview and package builds', 'Nov 18'],
      ['3.0.1', 'Windows · Tray and audio recovery fixes', 'Nov 21'],
      ['3.1.1', 'Linux · Autostart and settings fixes', 'Nov 21'],
    ];
    const top = head.y;
    const first = top + 74;
    const height = 308;
    let body = head.s + group(2, card(40, top, 500, height, 20) + label(64, top + 34, 'RELEASED', A.hi));
    body += `<line x1="74" y1="${first}" x2="74" y2="${first + 42 * (releases.length - 1)}" stroke="${A.hi}" stroke-opacity=".35" stroke-width="2"/>`;
    releases.forEach(([version, title, date], i) => {
      const y = first + i * 42;
      const latest = i === releases.length - 1;
      body += group(3 + i, (latest ? `<circle cx="74" cy="${y}" r="11" fill="${A.hi}" fill-opacity=".18"/>` : '')
        + dot(74, y, 5)
        + text(96, y + 4, version, { maxWidth: 50, size: 13, weight: 700, mono: true, fill: latest ? A.grad[0] : P.ink })
        + text(156, y + 5, title, { maxWidth: 300, size: 15, weight: 700 })
        + text(516, y + 4, date, { maxWidth: 48, size: 12, mono: true, anchor: 'end', fill: P.muted }));
    });
    const blocks = [
      ['NEXT', 0, 178, A.hi, [
        ['GNOME desktop', 'Check tray, themes and hardware.'],
        ['Fedora and Debian', 'Test clean package installations.'],
      ]],
      ['KNOWN GAP', 194, 114, P.muted, [
        ['Linux display controls', 'Explore desktop and compositor APIs.'],
      ]],
    ];
    blocks.forEach(([heading, offset, h, color, items], i) => {
      const y = top + offset;
      let block = card(556, y, 284, h, 20) + label(580, y + 34, heading, color);
      items.forEach(([title, detail], j) => {
        const row = y + 66 + j * 54;
        block += `<circle cx="586" cy="${row - 5}" r="4.5" fill="none" stroke="${color}" stroke-width="2"${i ? ' stroke-dasharray="2.4 2.4"' : ''}/>`
          + text(602, row, title, { maxWidth: 214, size: 14, weight: 700 })
          + lines(602, row + 19, wrap(detail, 214, 12.5), { size: 12.5, lh: 17, fill: P.muted });
      });
      body += group(3 + i, block);
    });
    return svg(880, top + height + 34, 'TrayMixer release timeline, upcoming Linux testing and the gap in integrated Linux display controls', body);
  }

  const linux = '<path d="M12 3c-2 0-3 2-3 4v2l-2 4-1 3-2 2 1 2h5l2-1 2 1h5l1-2-2-2-1-3-2-4V7c0-2-1-4-3-4z"/><path d="m10 9 2 1 2-1m-4-2h.1m3.9 0h.1"/>';
  return {
    'hero-en.svg': hero(),
    'btn-windows-en.svg': button('Download for Windows', ICONS.windows, true),
    'btn-linux-en.svg': button('Download for Linux', linux, true),
    'btn-portable-en.svg': button('Windows portable', ICONS.download, true),
    'btn-new-en.svg': button('What’s new', ICONS.spark, false),
    'features-en.svg': features(),
    'keys-en.svg': controls(),
    'states-en.svg': states(),
    'numbers-en.svg': numbers(),
    'roadmap-en.svg': roadmap(),
  };
}
