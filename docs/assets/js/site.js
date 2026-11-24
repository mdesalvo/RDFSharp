(function () {
  var css = getComputedStyle(document.documentElement);
  function token(name) { return css.getPropertyValue(name).trim(); }
  var still = matchMedia('(prefers-reduced-motion: reduce)').matches;

  // The web behind the page: resources (circles) and literals (squares) drifting slowly, joined by a faint edge whenever
  // two pass close, as triples join terms. Few enough to cost nothing, paused while the tab is hidden.
  var web = document.querySelector('.web');
  if (web && !still) {
    var ctx = web.getContext('2d');
    var nodeRgb = token('--web-node'), litRgb = token('--web-lit'), edgeRgb = token('--web-edge');
    var terms = [], w = 0, h = 0, reach = 150;
    function size() {
      var ratio = window.devicePixelRatio || 1;
      w = window.innerWidth; h = window.innerHeight;
      web.width = w * ratio; web.height = h * ratio;
      ctx.setTransform(ratio, 0, 0, ratio, 0, 0);
    }
    size();
    var count = w < 640 ? 14 : 34;
    for (var n = 0; n < count; n++) {
      terms.push({
        x: Math.random() * w, y: Math.random() * h,
        vx: (Math.random() - .5) * .16, vy: (Math.random() - .5) * .16,
        r: 2 + Math.random() * 2.2, literal: Math.random() < .35
      });
    }
    window.addEventListener('resize', size);
    var running = true;
    document.addEventListener('visibilitychange', function () { running = !document.hidden; if (running) { requestAnimationFrame(frame); } });
    function frame() {
      if (!running) { return; }
      ctx.clearRect(0, 0, w, h);
      for (var i = 0; i < terms.length; i++) {
        var a = terms[i];
        a.x += a.vx; a.y += a.vy;
        if (a.x < -20) { a.x = w + 20; } else if (a.x > w + 20) { a.x = -20; }
        if (a.y < -20) { a.y = h + 20; } else if (a.y > h + 20) { a.y = -20; }
        for (var j = i + 1; j < terms.length; j++) {
          var b = terms[j], dx = a.x - b.x, dy = a.y - b.y, d = Math.sqrt(dx * dx + dy * dy);
          if (d < reach) {
            ctx.strokeStyle = 'rgba(' + edgeRgb + ',' + (.22 * (1 - d / reach)) + ')';
            ctx.lineWidth = 1;
            ctx.beginPath(); ctx.moveTo(a.x, a.y); ctx.lineTo(b.x, b.y); ctx.stroke();
          }
        }
      }
      for (var k = 0; k < terms.length; k++) {
        var t = terms[k];
        ctx.fillStyle = 'rgba(' + (t.literal ? litRgb : nodeRgb) + ',.5)';
        if (t.literal) { ctx.fillRect(t.x - t.r, t.y - t.r, t.r * 2, t.r * 2); }
        else { ctx.beginPath(); ctx.arc(t.x, t.y, t.r, 0, Math.PI * 2); ctx.fill(); }
      }
      requestAnimationFrame(frame);
    }
    requestAnimationFrame(frame);
  }

  // The menu: a drawer below 1000px, closed by the shade, by Escape or by choosing a page.
  var menu = document.querySelector('.menu');
  var side = document.getElementById('side');
  var shade = document.querySelector('.shade');
  function setOpen(open) {
    side.classList.toggle('open', open);
    shade.hidden = !open;
    menu.setAttribute('aria-expanded', open ? 'true' : 'false');
    document.body.style.overflow = open ? 'hidden' : '';
  }
  menu.addEventListener('click', function () { setOpen(!side.classList.contains('open')); });
  shade.addEventListener('click', function () { setOpen(false); });
  side.querySelectorAll('nav a').forEach(function (a) { a.addEventListener('click', function () { setOpen(false); }); });

  // The current page stays in view inside a long menu.
  var here = side.querySelector('a.here');
  if (here) { here.scrollIntoView({ block: 'center' }); window.scrollTo(0, 0); }

  // Every table scrolls inside its own box, so a phone never scrolls the page sideways.
  document.querySelectorAll('.prose table').forEach(function (t) {
    if (t.parentElement.classList.contains('table')) { return; }
    var box = document.createElement('div');
    box.className = 'table';
    t.parentNode.insertBefore(box, t);
    box.appendChild(t);
  });

  // Diagrams: a fenced mermaid block becomes a graph drawn in the manual's own notation. Mermaid is fetched only by the
  // pages that have one, and it takes its colours from the theme the reader is in.
  // Rouge has no lexer for mermaid, so the block arrives as a bare pre with the language on its code.
  var charts = document.querySelectorAll('.prose code.language-mermaid');
  if (charts.length) {
    charts.forEach(function (code) {
      var block = code.closest('.highlighter-rouge') || code.parentNode;
      var fig = document.createElement('div');
      fig.className = 'mermaid figure';
      fig.textContent = code.textContent;
      block.parentNode.replaceChild(fig, block);
    });
    var tag = document.createElement('script');
    tag.src = 'https://cdn.jsdelivr.net/npm/mermaid@11.4.1/dist/mermaid.min.js';
    tag.onload = function () {
      window.mermaid.initialize({
        startOnLoad: false, theme: 'base', securityLevel: 'strict',
        flowchart: { curve: 'basis', htmlLabels: true, padding: 10, useMaxWidth: false, subGraphTitleMargin: { top: 6, bottom: 10 } },
        themeVariables: {
          fontFamily: token('--mono'), fontSize: '13px',
          background: 'transparent', primaryColor: token('--surface'), primaryTextColor: token('--ink'),
          primaryBorderColor: token('--accent'), lineColor: token('--faint'), textColor: token('--soft'),
          edgeLabelBackground: token('--night'), clusterBkg: token('--raised'), clusterBorder: token('--line'), titleColor: token('--soft'),
          tertiaryColor: token('--raised')
        },
        themeCSS: '.res > *{stroke:' + token('--accent') + ' !important;fill:' + token('--surface') + ' !important}' +
                  '.lit > *{stroke:' + token('--literal') + ' !important;fill:' + token('--surface') + ' !important}' +
                  '.lit .nodeLabel,.lit text{fill:' + token('--literal') + ' !important;color:' + token('--literal') + ' !important}' +
                  '.cluster-label text,.cluster-label span{fill:' + token('--soft') + ' !important;color:' + token('--soft') + ' !important}' +
                  '.blank > *{stroke:' + token('--faint') + ' !important;stroke-dasharray:4 3 !important;fill:' + token('--surface') + ' !important}'
      });
      window.mermaid.run({ querySelector: '.mermaid' });
    };
    document.head.appendChild(tag);
  }

  // Pairs: the same thing in the standard language and in C#. The reader's last choice of language holds on every
  // pair of every page, so whoever reads in C# keeps reading in C#.
  var names = { turtle: 'Turtle', sparql: 'SPARQL', csharp: 'C#', xml: 'RDF/XML', json: 'JSON', plaintext: 'Text', shell: 'Shell' };
  var preferred = null;
  try { preferred = localStorage.getItem('rdfsharp-lang'); } catch (e) { preferred = null; }
  var pairs = [];
  document.querySelectorAll('.prose .pair').forEach(function (pair) {
    var blocks = Array.prototype.filter.call(pair.children, function (c) { return c.classList.contains('highlighter-rouge'); });
    if (blocks.length < 2) { return; }
    var given = (pair.getAttribute('data-labels') || '').split(',');
    var tabs = document.createElement('div');
    tabs.className = 'tabs';
    tabs.setAttribute('role', 'tablist');
    var panes = blocks.map(function (block, i) {
      var lang = (block.className.match(/language-(\S+)/) || [])[1] || 'plaintext';
      var label = (given[i] || '').trim() || names[lang] || lang;
      var pane = document.createElement('div');
      pane.className = 'pane';
      pane.setAttribute('data-label', label);
      var caption = document.createElement('div');
      caption.className = 'label';
      caption.textContent = label;
      pair.insertBefore(pane, block);
      pane.appendChild(caption);
      pane.appendChild(block);
      var b = document.createElement('button');
      b.type = 'button';
      b.setAttribute('role', 'tab');
      b.textContent = label;
      b.addEventListener('click', function () {
        try { localStorage.setItem('rdfsharp-lang', label); } catch (e) { /* the choice just lasts for this page */ }
        pairs.forEach(function (p) { p.show(label); });
      });
      tabs.appendChild(b);
      return pane;
    });
    pair.insertBefore(tabs, pair.firstChild);
    // Side by side only when every line of both languages fits in half the column, so no code is ever cut.
    var longest = panes.reduce(function (most, p) {
      return Math.max(most, p.querySelector('code').textContent.split('\n').reduce(function (m, l) { return Math.max(m, l.length); }, 0));
    }, 0);
    function fit() { pair.classList.toggle('wide', panes.length === 2 && longest * 8.2 + 44 <= pair.clientWidth / 2); }
    fit();
    window.addEventListener('resize', fit);
    var entry = {
      show: function (label) {
        var at = panes.findIndex(function (p) { return p.getAttribute('data-label') === label; });
        if (at < 0) { return; }
        panes.forEach(function (p, i) { p.classList.toggle('on', i === at); tabs.children[i].setAttribute('aria-selected', i === at ? 'true' : 'false'); });
      }
    };
    entry.show(panes[0].getAttribute('data-label'));
    if (preferred) { entry.show(preferred); }
    pairs.push(entry);
  });

  // A copy button on every code block.
  document.querySelectorAll('.prose pre').forEach(function (pre) {
    if (pre.closest('.mermaid')) { return; }
    var b = document.createElement('button');
    b.className = 'copy';
    b.type = 'button';
    b.textContent = 'Copy';
    b.addEventListener('click', function () {
      var code = pre.querySelector('code') || pre;
      navigator.clipboard.writeText(code.innerText.trim()).then(function () {
        b.textContent = 'Copied';
        setTimeout(function () { b.textContent = 'Copy'; }, 1500);
      });
    });
    pre.appendChild(b);
  });

  // On this page: the sections of a long page in a rail on very wide screens, the one being read marked.
  var heads = document.querySelectorAll('.prose h2[id]');
  var toc = document.querySelector('.toc');
  if (toc && heads.length >= 3) {
    var list = toc.querySelector('nav');
    var links = Array.prototype.map.call(heads, function (h) {
      var a = document.createElement('a');
      a.href = '#' + h.id;
      a.textContent = h.textContent;
      list.appendChild(a);
      return a;
    });
    toc.hidden = false;
    document.body.classList.add('has-toc');
    window.dispatchEvent(new Event('resize')); // the rail narrows the column, so the pairs must fit again
    // The section being read is the last one whose heading has passed the upper third of the window. At the foot of
    // the page the last section wins: its heading may never climb that far, because the page ends before it can.
    var mark = function () {
      var at = 0;
      var line = window.innerHeight / 3;
      heads.forEach(function (h, i) { if (h.getBoundingClientRect().top <= line) at = i; });
      if (window.innerHeight + window.scrollY >= document.documentElement.scrollHeight - 2) at = heads.length - 1;
      links.forEach(function (a, i) { a.classList.toggle('here', i === at); });
    };
    window.addEventListener('scroll', mark, { passive: true });
    window.addEventListener('resize', mark);
    mark();
  }

  // Search over search.json: a title match ranks above a body match, each hit shows where it matched and the arrow
  // keys walk the hits.
  var input = document.getElementById('q');
  var hits = document.getElementById('hits');
  var index = null, cursor = -1;
  function load(done) {
    if (index) { return done(); }
    fetch(window.RDFSHARP_SEARCH).then(function (r) { return r.json(); })
      .then(function (data) { index = data; done(); })
      .catch(function () { index = []; done(); });
  }
  function esc(s) { return s.replace(/[&<>"]/g, function (c) { return { '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;' }[c]; }); }
  function run() {
    var words = input.value.toLowerCase().split(/\s+/).filter(function (t) { return t.length > 1; });
    cursor = -1;
    if (!words.length) { hits.hidden = true; return; }
    var found = [];
    index.forEach(function (page) {
      var title = page.title.toLowerCase(), text = page.text.toLowerCase(), score = 0, first = -1;
      for (var i = 0; i < words.length; i++) {
        var inTitle = title.indexOf(words[i]) >= 0, at = text.indexOf(words[i]);
        if (!inTitle && at < 0) { return; }
        score += (inTitle ? 10 : 0) + (at >= 0 ? 1 : 0);
        if (first < 0 && at >= 0) { first = at; }
      }
      var from = Math.max(0, first - 50);
      found.push({ page: page, score: score, excerpt: first >= 0 ? (from > 0 ? '…' : '') + page.text.substr(from, 130) + '…' : '' });
    });
    found.sort(function (a, b) { return b.score - a.score; });
    hits.innerHTML = found.length
      ? found.slice(0, 8).map(function (h) { return '<a href="' + h.page.url + '"><strong>' + esc(h.page.title) + '</strong><span>' + esc(h.excerpt) + '</span></a>'; }).join('')
      : '<div class="none">Nothing found for these words.</div>';
    hits.hidden = false;
  }
  function mark(step) {
    var all = hits.querySelectorAll('a');
    if (!all.length) { return; }
    cursor = (cursor + step + all.length) % all.length;
    all.forEach(function (a, i) { a.classList.toggle('active', i === cursor); });
    all[cursor].scrollIntoView({ block: 'nearest' });
  }
  input.addEventListener('input', function () { load(run); });
  input.addEventListener('keydown', function (e) {
    if (e.key === 'ArrowDown') { e.preventDefault(); mark(1); }
    if (e.key === 'ArrowUp') { e.preventDefault(); mark(-1); }
    if (e.key === 'Enter') {
      var pick = hits.querySelectorAll('a')[cursor < 0 ? 0 : cursor];
      if (pick) { window.location.href = pick.href; }
    }
  });
  document.addEventListener('click', function (e) { if (!e.target.closest('.search')) { hits.hidden = true; } });
  document.addEventListener('keydown', function (e) {
    var typing = /input|textarea/i.test((document.activeElement || {}).tagName || '');
    if (e.key === '/' && !typing) {
      e.preventDefault();
      if (window.innerWidth < 1000) { setOpen(true); }
      input.focus();
    }
    if (e.key === 'Escape') { hits.hidden = true; setOpen(false); }
  });
})();
