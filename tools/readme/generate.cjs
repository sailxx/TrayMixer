// Layout and embedded typography adapted from sailxx/Tobi's README artwork.
// Reference snapshots are data, not executable code. Rebuild: node tools/readme/generate.cjs
const fs=require('node:fs');const path=require('node:path');
const dir=path.resolve(__dirname,'../../assets/readme');
const ref=fs.readFileSync(path.join(__dirname,'reference/features.svg'),'utf8');
const btn=fs.readFileSync(path.join(__dirname,'reference/button.svg'),'utf8');
const esc=s=>String(s).replaceAll('&','&amp;').replaceAll('<','&lt;').replaceAll('"','&quot;');
const recolor=s=>s.replaceAll('#2E9BFF','#4CC2FF').replaceAll('#1478F0','#229BDC').replaceAll('#5AB8FF','#74D3FF').replaceAll('#7CC4FF','#A8E1FF').replaceAll('#6CBBFF','#8BDAFF').replaceAll('#1F7BF0','#36ADEB');
const style=recolor(ref.match(/<style>[\s\S]*?<\/style>/)[0]);
const defs=recolor(ref.match(/<defs>[\s\S]*?<\/defs>/)[0]);
const text=(x,y,s,size=15,fill='#A3A3A3',weight=500,mono=false,extra='')=>`<text ${mono?'class="m" ':''}x="${x}" y="${y}" font-size="${size}" font-weight="${weight}" fill="${fill}" ${extra}>${esc(s)}</text>`;
const rect=(x,y,w,h,r,fill,stroke='none')=>`<rect x="${x}" y="${y}" width="${w}" height="${h}" rx="${r}" fill="${fill}" stroke="${stroke}"/>`;
const card=(x,y,w,h,r=20)=>rect(x,y,w,h,r,'url(#cg)','#2A2A2A')+rect(x+r,y+.5,w-r*2,1,0,'url(#hl)');
const chip=(x,y,s,w=125)=>rect(x,y,w,30,8,'#141414','#333')+`<path d="M${x+13} ${y+15.5}l4 4 8-8.5" fill="none" stroke="#4CC2FF" stroke-width="2.2" stroke-linecap="round"/>`+text(x+32,y+20,s,12,'#fff',600,true,'letter-spacing="1"');
const head=(label,a,b)=>chip(40,34,label)+text(40,104,a,32,'#fff',800,false,'letter-spacing="-.6"')+text(40,104,b,32,'url(#tg)',800,false,'letter-spacing="-.6"');
function frame(w,h){return `<g clip-path="url(#clip)">${rect(0,0,w,h,24,'url(#bg)')}<circle class="glow" cx="${w*.82}" cy="${Math.min(h*.2,120)}" r="${Math.min(h*.7,340)}" fill="url(#g1)"/><circle class="glow2" cx="${w*.1}" cy="${h*.95}" r="${Math.min(h*.6,300)}" fill="url(#g2)"/><rect class="sweep" width="${w*.35}" height="${h}" fill="url(#sw)"/></g>`+rect(.5,.5,w-1,h-1,24,'none','#242424');}
function svg(file,w,h,label,body,framed=true){fs.writeFileSync(path.join(dir,file),`<svg xmlns="http://www.w3.org/2000/svg" width="${w}" height="${h}" viewBox="0 0 ${w} ${h}" role="img" aria-label="${esc(label)}">${style}${defs.replace('width="880" height="596"',`width="${w}" height="${h}"`)}${framed?frame(w,h):''}${body}</svg>\n`);}
// Preserve Tobi's exact features layout, numbered circles and header structure.
const replacements=[
 ['All about your agent —','Everyday adjustments —'],[' in one little window',' in one tray panel'],
 ['Live agents','Your audio, separately'],['Claude Code, Codex, Cursor, Gemini CLI,','Control each output and application.'],['Copilot CLI, OpenCode, Amp and more.','Turn a game down without losing a call.'],
 ['Permissions','Windows display tools'],['Allow, Deny and Always right in the window.','Brightness, blue-light filtering and'],['Answer the agent’s questions with a click.','software dimming alongside your audio.'],
 ['Terminal and VS Code','Built for the desktop'],['Ctrl+Alt+T brings the session’s window up.','Tray access, wheel adjustments and mute.'],['See a diff and open the file in your editor.','Hide rows you do not need.'],
 ['GitHub and services','A look that fits'],['Open PRs, review requests and CI status.','Rounded Windows flyout; Linux themes'],['Vercel, Stripe, n8n, Notion, Cal.com.','follow GNOME or your chosen palette.'],
 ['Chat with models','Local by design'],['Claude, OpenAI, Gemini, DeepSeek, Ollama.','No account, advertising or analytics.'],['Drop a file on Tobi and ask about it.','Preferences stay on your computer.'],
 ['A real character','Open and inspectable'],['Blinks, follows your cursor, cheers,','Apache 2.0 source and build instructions.'],['dozes off and grumbles when poked.','Read the changes in each platform release.']
];
let features=recolor(ref).replace(/aria-label="[^"]*"/,'aria-label="TrayMixer benefits: audio, Windows display tools, tray shortcuts, themes, local settings and open source"');
for(const [a,b] of replacements)features=features.replace(esc(a),esc(b));
fs.writeFileSync(path.join(dir,'benefits-blue.svg'),features);
// Same timeline-left / stacked next-and-later-right composition as Tobi.
let road=chip(40,34,'ROADMAP',116)+`<text x="40" y="104" font-size="32" font-weight="800" letter-spacing="-.6" fill="#fff">A working release —<tspan fill="url(#tg)"> and what comes next</tspan></text>`;
road+=card(40,132,500,420)+text(64,166,'AVAILABLE',12,'#4CC2FF',600,true,'letter-spacing="1"');
const shipped=[['3.0','Windows stable'],['','Device and application volume'],['','Brightness and blue-light tools'],['','Tray controls and hidden rows'],['3.1','Linux preview'],['','Audio, GNOME colors and themes'],['','DEB, RPM and AppImage']];
road+='<line x1="74" y1="206" x2="74" y2="506" stroke="#4CC2FF" stroke-opacity=".35" stroke-width="2"/>';
shipped.forEach(([v,s],i)=>{const y=206+i*50;road+=`<circle cx="74" cy="${y}" r="${v?5:3}" fill="#4CC2FF"/>`+text(96,y+4,v,13,'#A8E1FF',700,true)+text(148,y+5,s,15,'#fff',700);});
road+=card(556,132,284,206)+text(580,166,'VALIDATE NEXT',12,'#4CC2FF',600,true,'letter-spacing="1"');
[['GNOME desktop','Tray, themes and real hardware'],['Fedora / Debian','Installation on clean systems']].forEach(([a,b],i)=>{const y=198+i*65;road+=`<circle cx="586" cy="${y-5}" r="4.5" fill="none" stroke="#4CC2FF" stroke-width="2"/>`+text(602,y,a,14,'#fff',700)+text(602,y+20,b,12.5);});
road+=card(556,354,284,198)+text(580,388,'EXPLORE LATER',12,'#A3A3A3',600,true,'letter-spacing="1"')+text(602,420,'Linux display controls',14,'#fff',700)+text(602,440,'Desktop-specific APIs needed.',12.5)+text(602,479,'Ongoing refinements',14,'#fff',700)+text(602,499,'Guided by bugs and feedback.',12.5)+text(40,583,'Priorities may change. No fixed delivery dates.',14);
svg('roadmap-blue.svg',880,616,'TrayMixer roadmap: available releases, validation priorities and future exploration without fixed dates',road);
let trust=chip(40,34,'TRUST',105)+`<text x="40" y="104" font-size="32" font-weight="800" letter-spacing="-.6" fill="#fff">Local by design.<tspan fill="url(#tg)"> Transparent about limits.</tspan></text>`;
[['Local settings','No account, ads or analytics.','Preferences stay on your computer.'],['Public source','Read the code and build instructions.','Apache 2.0; no security audit claimed.'],['Verify downloads','Unsigned releases: check the source','and compare published checksums.']].forEach(([title,a,b],i)=>{const x=40+i*272;trust+=card(x,132,256,144)+text(x+20,167,title,19,'#fff',700)+text(x+20,207,a,13)+text(x+20,229,b,13);});
svg('trust-blue.svg',880,312,'Local settings, public source and unsigned downloads: verify files and read the documented limitations',trust);
const windowIcon=btn.match(/<g transform="translate\(18 14\)"[\s\S]*?<\/g>/)[0];
for(const [name,w,label,primary,glyph] of [['download-windows',266,'Download for Windows 11',true,'windows'],['download-linux',202,'Linux packages',true,'linux'],['release-notes',174,'Release notes',false,'arrow'],['report-issue',184,'Report an issue',false,'arrow']]){
const icon=glyph==='windows'?windowIcon:`<g transform="translate(18 14)" fill="none" stroke="${primary?'#021226':'#fff'}" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">${glyph==='linux'?'<path d="M12 3c-3 0-4 3-4 6l-3 8 7 4 7-4-3-8c0-3-1-6-4-6z"/>':'<path d="M4 12h16m-6-6 6 6-6 6"/>'}</g>`;
svg(name+'.svg',w,52,label,`<defs><linearGradient id="b" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="${primary?'#8BDAFF':'#202020'}"/><stop offset="1" stop-color="${primary?'#36ADEB':'#101010'}"/></linearGradient></defs>`+rect(.5,.5,w-1,51,14,'url(#b)',primary?'#8BDAFF':'#333')+icon+text(50,32,label,15,primary?'#021226':'#fff',700),false);
}
for(const [name,w,label] of [['badge-windows',240,'Windows 11 · 3.0.1 stable'],['badge-linux',216,'Linux · 3.1.1 preview'],['badge-license',194,'Apache 2.0 · open']]) svg(name+'.svg',w,36,label,rect(.5,.5,w-1,35,12,'#141414','#333')+'<circle cx="17" cy="18" r="4" fill="#4CC2FF"/>'+text(30,23,label,13,'#A8E1FF',600,true),false);
svg('logo-blue.svg',144,144,'TrayMixer logo: three blue mixer sliders',card(4,4,136,136,32)+[42,72,102].map((y,i)=>`<path d="M28 ${y}h88" stroke="#454545" stroke-width="6" stroke-linecap="round"/><path d="M28 ${y}h${[55,28,68][i]}" stroke="#4CC2FF" stroke-width="6" stroke-linecap="round"/><circle cx="${[83,56,96][i]}" cy="${y}" r="10" fill="#252525" stroke="#5b5b5b"/><circle cx="${[83,56,96][i]}" cy="${y}" r="6" fill="#4CC2FF"/>`).join(''),false);
require('./cards.cjs');
console.log('PASS: README assets generated using Tobi typography, frames, numbered feature grid, timeline and buttons');
