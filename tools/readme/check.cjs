const fs = require('node:fs');
const path = require('node:path');
const assert = require('node:assert/strict');
const root = path.resolve(__dirname, '../..');
const md = fs.readFileSync(path.join(root, 'README.md'), 'utf8');
const refs = [...md.matchAll(/\]\(([^)]+)\)|\bsrc="([^"]+)"/g)].map(m=>m[1]||m[2]);
const local = refs.filter(r=>!/^https?:|^#/.test(r));
for(const ref of local) assert(fs.existsSync(path.join(root, ref.split('#')[0])), `Missing local reference: ${ref}`);
const anchors = [...md.matchAll(/^#{1,6} (.+)$/gm)].map(m=>m[1].toLowerCase().replace(/[^\w\s-]/g,'').replace(/ /g,'-'));
for(const ref of refs.filter(r=>r.startsWith('#'))) assert(anchors.includes(ref.slice(1)), `Missing anchor: ${ref}`);
assert.equal((md.match(/<details>/g)||[]).length, (md.match(/<\/details>/g)||[]).length);
assert.equal((md.match(/^```/gm)||[]).length%2, 0);
assert(!md.includes('gh attestation verify TrayMixer-Setup.exe'), 'Unsupported installer provenance claim');
for(const s of ['Actively in development', 'Vlad', '## Roadmap', '## Privacy & security', '## Reliability & current limits']) assert(md.includes(s), `Missing section ${s}`);
const assets = [...new Set(local.filter(r=>r.endsWith('.svg')))];
for(const asset of assets) {
 const s=fs.readFileSync(path.join(root,asset),'utf8');
 assert(s.startsWith('<svg ') && s.includes('</svg>'), `Invalid SVG root: ${asset}`);
 assert(!/<script|<foreignObject|https?:\/\/(?!www.w3.org)/.test(s), `Nonportable SVG: ${asset}`);
 assert(s.includes('aria-label='), `No accessible name: ${asset}`);
}
const cards = [...md.matchAll(/<img src="(assets\/readme\/(?:benefit|roadmap|trust)-\d\.svg)" width="320" alt="[^"]+">/g)];
assert.equal(cards.length, 12, 'All informational panels must use separate wrapping cards');
assert(!/!\[[^\]]*\]\(assets\/readme\/(?:benefits|roadmap|trust)-blue\.svg\)/.test(md), 'Wide panels shrink text on phones');
for (const [, asset] of cards) {
 const svg = fs.readFileSync(path.join(root, asset), 'utf8');
 assert(svg.includes('viewBox="0 0 320 '), 'Cards must fit narrow README columns');
 const sizes = [...svg.matchAll(/font-size="([\d.]+)"/g)].map(m => Number(m[1]));
 assert(sizes.length && Math.min(...sizes) >= 17, 'Card text must remain readable at native width');
}
console.log('PASS: 12 wrapping cards, 320px viewBox, minimum 17px text; no wide information panels');
console.log(`PASS: ${local.length} local references, ${refs.filter(r=>r.startsWith('#')).length} anchors, balanced details/fences, ${assets.length} self-contained named SVGs`);
// A local layout preview, not a substitute for GitHub's own Markdown renderer.
const escape=s=>s.replaceAll('&','&amp;').replaceAll('<','&lt;').replaceAll('>','&gt;');
const inline=s=>{const images=[];return s.replace(/!\[([^\]]*)\]\(([^)]+)\)/g,(_,alt,src)=>{images.push(`<img alt="${alt}" src="${src}">`);return `IMAGETOKEN${images.length-1}END`;}).replace(/\[([^\]]+)\]\(([^)]+)\)/g,'<a href="$2">$1</a>').replace(/IMAGETOKEN(\d+)END/g,(_,i)=>images[i]).replace(/\*\*(.*?)\*\*/g,'<strong>$1</strong>').replace(/`([^`]+)`/g,(_,s)=>'<code>'+escape(s)+'</code>');};
let code=false,table=false,body='';
for(const line of md.replace(/\n(?=\[!\[)/g, ' ').split('\n')) {
 if(line.startsWith('```')) {body+=code?'</code></pre>':'<pre><code>';code=!code;continue;}
 if(code){body+=escape(line)+'\n';continue;}
 if(line.startsWith('|')) {
  if(!table){body+='<table>';table=true;}
  if(!/^\|[-| ]+\|$/.test(line))body+='<tr>'+line.slice(1,-1).split('|').map(c=>'<td>'+inline(c.trim())+'</td>').join('')+'</tr>';
  continue;
 }
 if(table){body+='</table>';table=false;}
 const heading=line.match(/^(#{1,6}) (.*)$/);
 if(heading){const id=heading[2].toLowerCase().replace(/[^\w\s-]/g,'').replace(/ /g,'-');body+=`<h${heading[1].length} id="${id}">${inline(heading[2])}</h${heading[1].length}>`;}
 else if(/^<\/?(?:div|p|details|summary|img)/.test(line))body+=line;
 else if(line.trim())body+='<p>'+inline(line.replace(/^> /,''))+'</p>';
}
const out=path.join(root,'.sharrowkin/readme-preview');fs.mkdirSync(out,{recursive:true});
fs.writeFileSync(path.join(out,'index.html'),`<!doctype html><html lang="en"><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>TrayMixer README — local layout preview</title><style>*{box-sizing:border-box}body{margin:0;background:#0d1117;color:#e6edf3;font:16px/1.6 -apple-system,BlinkMacSystemFont,"Segoe UI",sans-serif}main{max-width:946px;margin:auto;padding:32px}img{max-width:100%;height:auto;vertical-align:middle}a{color:#74d3ff;text-decoration:none}a{display:inline-block;min-height:44px;padding:8px 2px}a:hover{text-decoration:underline}h1,h2,h3{line-height:1.25}h1{font-size:32px}h2{font-size:24px;margin-top:32px;border-bottom:1px solid #30363d;padding-bottom:8px}h3{font-size:20px}p{margin:12px 0}td{padding:8px 13px;border:1px solid #30363d}table{border-collapse:collapse;display:block;overflow:auto}pre{overflow:auto;padding:16px;background:#161b22;border-radius:6px}summary{cursor:pointer;padding:8px 0}code{font-size:13px}div[align] a{display:inline-block;margin:3px 0}a:focus-visible,summary:focus-visible{outline:2px solid #74d3ff;outline-offset:4px}@media(max-width:600px){main{padding:20px}}</style><main>${body}</main><script>document.querySelectorAll('a').forEach(a=>{const label=a.textContent.trim()||a.querySelector('img')?.alt;if(label)a.setAttribute('aria-label',label);});</script></html>`);
console.log('PASS: local layout preview generated (GitHub rendering still requires separate acceptance)');
