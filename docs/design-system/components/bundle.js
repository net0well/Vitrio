/* @ds-bundle: {"format":4,"namespace":"Vitrio","components":[{"name":"Icon"},{"name":"Button"},{"name":"IconButton"},{"name":"SearchField"},{"name":"TextField"},{"name":"TextArea"},{"name":"Chip"},{"name":"SegmentedControl"},{"name":"Switch"},{"name":"RatingInput"},{"name":"PaymentMethodPicker"},{"name":"Badge"},{"name":"Avatar"},{"name":"Rating"},{"name":"CategoryTile"},{"name":"HeroCard"},{"name":"ProviderCard"},{"name":"ReviewCard"},{"name":"StatusTimeline"},{"name":"QuoteCard"},{"name":"PaymentReceipt"},{"name":"BottomNav"},{"name":"TopBar"},{"name":"ActionBar"}]} */
(function () {
  var React = window.React;
  var h = React.createElement;
  var useState = React.useState;
  var useId = React.useId;
  var ICONS = {"search":[["circle",{"cx":11,"cy":11,"r":7}],["path",{"d":"M20 20l-3.5-3.5"}]],"pin":[["path",{"d":"M12 21s-7-6.2-7-11.5A7 7 0 0 1 19 9.5C19 14.8 12 21 12 21z"}],["circle",{"cx":12,"cy":9.5,"r":2.5}]],"star":[["path",{"d":"M12 3.5l2.6 5.3 5.8.8-4.2 4.1 1 5.8L12 16.8l-5.2 2.7 1-5.8-4.2-4.1 5.8-.8z"}]],"home":[["path",{"d":"M4 10.5L12 4l8 6.5V20a1 1 0 0 1-1 1h-4.5v-6h-5v6H5a1 1 0 0 1-1-1z"}]],"orders":[["rect",{"x":5,"y":4,"width":14,"height":17,"rx":2}],["path",{"d":"M9 4h6v3H9zM9 12h6M9 16h4"}]],"user":[["circle",{"cx":12,"cy":8,"r":4}],["path",{"d":"M4 21c1.5-4 4.5-6 8-6s6.5 2 8 6"}]],"chart":[["path",{"d":"M5 20v-7M12 20V5M19 20v-10"}]],"chevron-left":[["path",{"d":"M15 18l-6-6 6-6"}]],"chevron-right":[["path",{"d":"M9 18l6-6-6-6"}]],"chevron-down":[["path",{"d":"M6 9l6 6 6-6"}]],"arrow-right":[["path",{"d":"M5 12h14M13 6l6 6-6 6"}]],"sliders":[["path",{"d":"M4 7h10M18 7h2M4 17h4M12 17h8"}],["circle",{"cx":16,"cy":7,"r":2}],["circle",{"cx":10,"cy":17,"r":2}]],"check":[["path",{"d":"M5 12.5l4.5 4.5L19 7.5"}]],"x":[["path",{"d":"M6 6l12 12M18 6L6 18"}]],"plus":[["path",{"d":"M12 5v14M5 12h14"}]],"shield-check":[["path",{"d":"M12 3l7 3v6c0 4.5-3 7.5-7 9-4-1.5-7-4.5-7-9V6z"}],["path",{"d":"M9 12l2 2 4-4"}]],"bell":[["path",{"d":"M6 16V11a6 6 0 0 1 12 0v5l1.5 2h-15z"}],["path",{"d":"M10 20a2 2 0 0 0 4 0"}]],"heart":[["path",{"d":"M12 20s-7-4.4-7-10a4 4 0 0 1 7-2.6A4 4 0 0 1 19 10c0 5.6-7 10-7 10z"}]],"share":[["circle",{"cx":18,"cy":5,"r":2.5}],["circle",{"cx":6,"cy":12,"r":2.5}],["circle",{"cx":18,"cy":19,"r":2.5}],["path",{"d":"M8.2 10.8l7.6-4.4M8.2 13.2l7.6 4.4"}]],"message":[["path",{"d":"M5 18.5L4 21l3.2-1.1A8.5 8.5 0 1 0 5 18.5z"}]],"send":[["path",{"d":"M21 3L10 14M21 3l-7 18-4-7-7-4z"}]],"camera":[["path",{"d":"M4 8h3l2-3h6l2 3h3v11H4z"}],["circle",{"cx":12,"cy":13,"r":3.5}]],"image":[["rect",{"x":3.5,"y":4.5,"width":17,"height":15,"rx":2}],["circle",{"cx":9,"cy":9.5,"r":1.8}],["path",{"d":"M20.5 16l-5-5-8.5 8.5"}]],"calendar":[["rect",{"x":4,"y":5,"width":16,"height":16,"rx":2}],["path",{"d":"M4 10h16M9 3v4M15 3v4"}]],"clock":[["circle",{"cx":12,"cy":12,"r":8.5}],["path",{"d":"M12 7.5V12l3 2"}]],"info":[["circle",{"cx":12,"cy":12,"r":8.5}],["path",{"d":"M12 11v5M12 8h.01"}]],"lock":[["rect",{"x":5,"y":11,"width":14,"height":9,"rx":2}],["path",{"d":"M8 11V8a4 4 0 0 1 8 0v3"}]],"flag":[["path",{"d":"M5 21V4M5 4h11l-2 4 2 4H5"}]],"wallet":[["rect",{"x":3.5,"y":6,"width":17,"height":13,"rx":2.5}],["path",{"d":"M3.5 10h17M15.5 14.5h2"}]],"qr":[["rect",{"x":4,"y":4,"width":6,"height":6,"rx":1}],["rect",{"x":14,"y":4,"width":6,"height":6,"rx":1}],["rect",{"x":4,"y":14,"width":6,"height":6,"rx":1}],["path",{"d":"M14 14h2v2h-2zM19 14h1M14 19v1M18 18h2v2h-2z"}]],"cash":[["rect",{"x":3,"y":6.5,"width":18,"height":11,"rx":2}],["circle",{"cx":12,"cy":12,"r":2.5}],["path",{"d":"M6.5 9.5h.01M17.5 14.5h.01"}]],"card":[["rect",{"x":3,"y":5.5,"width":18,"height":13,"rx":2.5}],["path",{"d":"M3 10h18M7 15h4"}]],"transfer":[["path",{"d":"M4 8h15l-3.5-3.5M20 16H5l3.5 3.5"}]],"bolt":[["path",{"d":"M13 3L5 13.5h6L10 21l8-10.5h-6z"}]],"droplet":[["path",{"d":"M12 3.5s-6 6.6-6 11a6 6 0 0 0 12 0c0-4.4-6-11-6-11z"}]],"brick":[["rect",{"x":3,"y":5,"width":18,"height":14,"rx":1.5}],["path",{"d":"M3 10h18M3 14.5h18M9 5v5M15 10v4.5M9 14.5V19"}]],"roller":[["rect",{"x":4,"y":3.5,"width":14,"height":5,"rx":1.5}],["path",{"d":"M18 6h2v5h-8v3"}],["rect",{"x":10.5,"y":14,"width":3,"height":7,"rx":1}]],"broom":[["path",{"d":"M15 3l-4 8.5"}],["path",{"d":"M6.5 11.5h8l2 9h-12z"}],["path",{"d":"M8.5 16v4.5M12 16v4.5"}]],"leaf":[["path",{"d":"M5 19c0-8 5-14 15-14 0 10-6 15-14 15"}],["path",{"d":"M5 19c3-4 6-6.5 9-8"}]],"wrench":[["path",{"d":"M14.5 6.5a4 4 0 0 0-5.3 5.3L4 17l3 3 5.2-5.2a4 4 0 0 0 5.3-5.3l-2.5 2.5-2.5-.5-.5-2.5z"}]],"grid":[["rect",{"x":4,"y":4,"width":6.5,"height":6.5,"rx":1.5}],["rect",{"x":13.5,"y":4,"width":6.5,"height":6.5,"rx":1.5}],["rect",{"x":4,"y":13.5,"width":6.5,"height":6.5,"rx":1.5}],["rect",{"x":13.5,"y":13.5,"width":6.5,"height":6.5,"rx":1.5}]]};
  var STAR_D = 'M12 3.5l2.6 5.3 5.8.8-4.2 4.1 1 5.8L12 16.8l-5.2 2.7 1-5.8-4.2-4.1 5.8-.8z';
  var RATING_WORDS = ['', 'Péssimo', 'Ruim', 'Regular', 'Bom', 'Excelente'];
  var PAYMENT_METHODS = [
    { value: 'pix', label: 'Pix', icon: 'qr' },
    { value: 'dinheiro', label: 'Dinheiro', icon: 'cash' },
    { value: 'debito', label: 'Cartão de débito', icon: 'card' },
    { value: 'credito', label: 'Cartão de crédito', icon: 'card' },
    { value: 'transferencia', label: 'Transferência', icon: 'transfer' },
    { value: 'outro', label: 'Outro', icon: 'wallet' }
  ];

  function cx() {
    return Array.prototype.filter.call(arguments, Boolean).join(' ');
  }

  function fmtRating(v) {
    return Number(v || 0).toFixed(1).replace('.', ',');
  }

  function fmtMoney(v) {
    if (typeof v === 'string') return v;
    return 'R$ ' + Number(v || 0).toFixed(2).replace('.', ',').replace(/\B(?=(\d{3})+(?!\d))/g, '.');
  }

  function useControlled(value, defaultValue, onChange) {
    var st = useState(defaultValue);
    var isControlled = value !== undefined;
    var current = isControlled ? value : st[0];
    function set(next) {
      if (!isControlled) st[1](next);
      if (onChange) onChange(next);
    }
    return [current, set];
  }

  function Icon(p) {
    var def = ICONS[p.name];
    if (!def) return null;
    var size = p.size || 20;
    var labelled = !!p.label;
    return h('svg', {
      className: cx('vt-icon', p.className),
      width: size,
      height: size,
      viewBox: '0 0 24 24',
      fill: p.filled ? 'currentColor' : 'none',
      stroke: 'currentColor',
      strokeWidth: p.strokeWidth || 2,
      strokeLinecap: 'round',
      strokeLinejoin: 'round',
      role: labelled ? 'img' : undefined,
      'aria-label': labelled ? p.label : undefined,
      'aria-hidden': labelled ? undefined : 'true',
      style: p.style
    }, def.map(function (el, i) {
      return h(el[0], Object.assign({ key: i }, el[1]));
    }));
  }

  function Star(p) {
    return h('svg', {
      className: p.empty ? 'vt-star-empty' : 'vt-star',
      width: p.size || 15,
      height: p.size || 15,
      viewBox: '0 0 24 24',
      'aria-hidden': 'true'
    }, h('path', { d: STAR_D }));
  }

  function Button(p) {
    var variant = p.variant || 'primary';
    var size = p.size || 'md';
    var iconSize = size === 'lg' ? 20 : 18;
    var className = cx('vt-btn', 'vt-btn-' + variant, size === 'lg' && 'vt-btn-lg', p.block && 'vt-btn-block', p.className);
    var children = [
      p.icon ? h(Icon, { key: 'i', name: p.icon, size: iconSize, strokeWidth: 2.2 }) : null,
      h('span', { key: 't' }, p.children),
      p.trailingIcon ? h(Icon, { key: 'ti', name: p.trailingIcon, size: iconSize, strokeWidth: 2.4, className: 'vt-btn-trail' }) : null
    ];
    if (p.href && !p.disabled) return h('a', { className: className, href: p.href }, children);
    return h('button', { type: p.type || 'button', className: className, disabled: p.disabled, onClick: p.onClick }, children);
  }

  function IconButton(p) {
    var variant = p.variant || 'surface';
    var className = cx('vt-iconbtn', variant !== 'surface' && 'vt-iconbtn-' + variant, p.className);
    var children = [
      h(Icon, { key: 'i', name: p.icon, size: p.iconSize || 20, strokeWidth: 2.2 }),
      p.dot ? h('span', { key: 'd', className: 'vt-iconbtn-dot', 'aria-hidden': 'true' }) : null
    ];
    if (p.href) return h('a', { className: className, href: p.href, 'aria-label': p.label }, children);
    return h('button', { type: 'button', className: className, 'aria-label': p.label, onClick: p.onClick }, children);
  }

  function SearchField(p) {
    var id = useId();
    function submit(e) {
      e.preventDefault();
      if (p.onSubmit) p.onSubmit(e.currentTarget.elements.q.value);
    }
    return h('form', { className: cx('vt-search', p.className), role: 'search', onSubmit: submit },
      h(Icon, { name: 'search', size: 20, strokeWidth: 2.2 }),
      h('label', { className: 'vt-sr', htmlFor: id }, p.label || 'Buscar serviço'),
      h('input', { id: id, name: 'q', type: 'search', placeholder: p.placeholder || 'Eletricista, diarista, pintor…', defaultValue: p.defaultValue }),
      h('button', { type: 'submit', className: 'vt-search-go', 'aria-label': 'Buscar' }, h(Icon, { name: 'arrow-right', size: 18, strokeWidth: 2.4 }))
    );
  }

  function TextField(p) {
    var id = useId();
    var hintId = id + '-hint';
    var state = useControlled(p.value, p.defaultValue || '', p.onChange);
    var hasHint = !!(p.error || p.hint || p.maxLength);
    return h('div', { className: cx('vt-field', p.error && 'vt-field-error', p.prefix === 'R$' && 'vt-field-money', p.className) },
      h('label', { className: 'vt-field-label', htmlFor: id }, p.label),
      h('div', { className: 'vt-field-box' },
        p.icon ? h(Icon, { name: p.icon, size: 20, strokeWidth: 2.2 }) : null,
        p.prefix ? h('span', { className: 'vt-field-prefix', 'aria-hidden': 'true' }, p.prefix) : null,
        h('input', {
          id: id,
          type: p.type || 'text',
          inputMode: p.inputMode,
          placeholder: p.placeholder,
          maxLength: p.maxLength,
          value: state[0],
          'aria-invalid': p.error ? 'true' : undefined,
          'aria-describedby': hasHint ? hintId : undefined,
          onChange: function (e) { state[1](e.target.value); }
        })
      ),
      hasHint ? h('div', { className: 'vt-field-hint', id: hintId },
        h('span', null, p.error || p.hint || ''),
        p.maxLength ? h('span', null, String(state[0]).length + '/' + p.maxLength) : null
      ) : null
    );
  }

  function TextArea(p) {
    var id = useId();
    var hintId = id + '-hint';
    var max = p.maxLength || 500;
    var state = useControlled(p.value, p.defaultValue || '', p.onChange);
    return h('div', { className: cx('vt-field', p.error && 'vt-field-error', p.className) },
      h('label', { className: 'vt-field-label', htmlFor: id }, p.label),
      h('textarea', {
        id: id,
        className: 'vt-textarea',
        placeholder: p.placeholder,
        maxLength: max,
        rows: p.rows || 4,
        value: state[0],
        'aria-invalid': p.error ? 'true' : undefined,
        'aria-describedby': hintId,
        onChange: function (e) { state[1](e.target.value); }
      }),
      h('div', { className: 'vt-field-hint', id: hintId },
        h('span', null, p.error || p.hint || ''),
        h('span', null, String(state[0]).length + '/' + max)
      )
    );
  }

  function Chip(p) {
    var state = useControlled(p.selected, !!p.defaultSelected, p.onChange);
    var on = !!state[0];
    return h('button', {
      type: 'button',
      className: cx('vt-chip', p.variant === 'soft' && 'vt-chip-soft', p.className),
      'aria-pressed': on ? 'true' : 'false',
      onClick: function () { state[1](!on); }
    }, on ? h(Icon, { name: 'check', size: 14, strokeWidth: 3 }) : null, p.children);
  }

  function SegmentedControl(p) {
    var options = p.options || [];
    var state = useControlled(p.value, p.defaultValue !== undefined ? p.defaultValue : (options[0] && options[0].value), p.onChange);
    return h('div', { className: cx('vt-seg', p.className), role: 'group', 'aria-label': p.label },
      options.map(function (o) {
        var on = o.value === state[0];
        return h('button', {
          key: o.value,
          type: 'button',
          className: 'vt-seg-btn',
          'aria-pressed': on ? 'true' : 'false',
          onClick: function () { state[1](o.value); }
        }, o.label);
      })
    );
  }

  function Switch(p) {
    var id = useId();
    var state = useControlled(p.checked, !!p.defaultChecked, p.onChange);
    var on = !!state[0];
    return h('div', { className: cx('vt-switch-row', p.className) },
      h('span', { className: 'vt-switch-text' },
        h('span', { className: 'vt-switch-label', id: id }, p.label),
        p.description ? h('span', { className: 'vt-switch-desc' }, p.description) : null
      ),
      h('button', {
        type: 'button',
        role: 'switch',
        className: 'vt-switch',
        'aria-checked': on ? 'true' : 'false',
        'aria-labelledby': id,
        onClick: function () { state[1](!on); }
      }, h('span', { className: 'vt-switch-knob' }))
    );
  }

  function RatingInput(p) {
    var state = useControlled(p.value, p.defaultValue || 0, p.onChange);
    var value = state[0];
    var stars = [];
    for (var i = 1; i <= 5; i++) {
      (function (n) {
        stars.push(h('button', {
          key: n,
          type: 'button',
          role: 'radio',
          className: 'vt-rate-btn',
          'aria-checked': value === n ? 'true' : 'false',
          'aria-label': n === 1 ? '1 estrela' : n + ' estrelas',
          onClick: function () { state[1](n); }
        }, h(Star, { size: 42, empty: n > value })));
      })(i);
    }
    return h('div', { className: cx('vt-rate', p.className) },
      h('div', { className: 'vt-rate-stars', role: 'radiogroup', 'aria-label': p.label || 'Nota de 1 a 5' }, stars),
      h('p', { className: 'vt-rate-label', 'aria-live': 'polite' }, RATING_WORDS[value] || '')
    );
  }

  function PaymentMethodPicker(p) {
    var methods = p.methods || PAYMENT_METHODS;
    var state = useControlled(p.value, p.defaultValue, p.onChange);
    return h('fieldset', { className: cx('vt-pay', p.className) },
      h('legend', { className: 'vt-pay-legend' }, p.label || 'Forma de pagamento'),
      h('div', { className: 'vt-pay-grid', role: 'radiogroup', 'aria-label': p.label || 'Forma de pagamento' },
        methods.map(function (m) {
          var on = m.value === state[0];
          return h('button', {
            key: m.value,
            type: 'button',
            role: 'radio',
            className: 'vt-pay-opt',
            'aria-checked': on ? 'true' : 'false',
            onClick: function () { state[1](m.value); }
          },
            h('span', { className: 'vt-pay-ico' }, h(Icon, { name: m.icon, size: 18, strokeWidth: 2.2 })),
            h('span', null, m.label)
          );
        })
      )
    );
  }

  function Badge(p) {
    var tone = p.tone || 'brand';
    return h('span', { className: cx('vt-badge', 'vt-badge-' + tone, p.className) },
      p.icon ? h(Icon, { name: p.icon, size: 14, strokeWidth: 2.4 }) : null,
      p.dot ? h('span', { className: 'vt-badge-dot', 'aria-hidden': 'true' }) : null,
      p.children
    );
  }

  function Avatar(p) {
    var size = p.size || 48;
    var kind = p.kind || 'provider';
    var radius = kind === 'client' ? '50%' : Math.round(size * 0.31) + 'px';
    return h('span', {
      className: cx('vt-avatar', 'vt-tone-' + (p.tone || 'plum'), p.className),
      style: { width: size, height: size, borderRadius: radius, fontSize: Math.round(size * 0.33) },
      role: 'img',
      'aria-label': p.name
    }, h('span', { 'aria-hidden': 'true' }, p.initials));
  }

  function Rating(p) {
    var value = p.value || 0;
    if (p.variant === 'stars') {
      var stars = [];
      for (var i = 1; i <= 5; i++) stars.push(h(Star, { key: i, size: p.size || 14, empty: i > Math.round(value) }));
      return h('span', { className: cx('vt-stars', p.className), role: 'img', 'aria-label': 'Nota ' + fmtRating(value) + ' de 5' }, stars);
    }
    return h('span', { className: cx('vt-rating', p.className) },
      h(Star, { size: p.size || 15 }),
      h('span', { className: 'vt-rating-value' }, h('span', { className: 'vt-sr' }, 'Nota '), fmtRating(value)),
      p.count !== undefined ? h('span', { className: 'vt-rating-meta' }, '(' + p.count + ')') : null,
      p.meta ? h('span', { className: 'vt-rating-meta' }, '· ' + p.meta) : null
    );
  }

  function CategoryTile(p) {
    return h('a', { className: cx('vt-cat', p.more && 'vt-cat-more', p.className), href: p.href || '#' },
      h('span', { className: 'vt-cat-ico' }, h(Icon, { name: p.icon, size: 20 })),
      h('span', null, p.label)
    );
  }

  function HeroCard(p) {
    return h('section', { className: cx('vt-hero', p.className) },
      p.icon ? h(Icon, { name: p.icon, size: 150, strokeWidth: 1.2, className: 'vt-hero-bg' }) : null,
      p.tag ? h(Badge, { tone: 'accent', dot: true }, p.tag) : null,
      h('h2', { className: 'vt-hero-title' }, p.title),
      p.text ? h('p', { className: 'vt-hero-text' }, p.text) : null,
      p.actionLabel ? h('a', { className: 'vt-hero-action', href: p.href || '#' }, p.actionLabel, h(Icon, { name: 'arrow-right', size: 16, strokeWidth: 2.4 })) : null
    );
  }

  function ProviderCard(p) {
    var badges = p.badges || [];
    return h('a', { className: cx('vt-pcard', p.className), href: p.href || '#' },
      h('span', { className: 'vt-pcard-top' },
        h(Avatar, { initials: p.initials, name: p.name, tone: p.tone, size: 60 }),
        h('span', { className: 'vt-pcard-info' },
          h('span', { className: 'vt-pcard-name' }, p.name, p.verified ? h(Icon, { name: 'shield-check', size: 16, strokeWidth: 2.2, label: 'Verificado' }) : null),
          h('span', { className: 'vt-pcard-prof' }, p.profession),
          h(Rating, { value: p.rating, count: p.reviews, meta: p.distance ? p.distance + ' · ' + p.neighborhood : p.neighborhood })
        )
      ),
      badges.length ? h('span', { className: 'vt-pcard-badges' }, badges.map(function (b, i) {
        return h(Badge, { key: i, tone: b.tone, icon: b.icon }, b.label);
      })) : null,
      h('span', { className: 'vt-pcard-foot' },
        h('span', { className: 'vt-pcard-from' },
          h('span', { className: 'vt-pcard-from-k' }, p.priceFrom ? 'A partir de' : 'Preço'),
          h('span', { className: 'vt-pcard-price' }, p.priceFrom ? fmtMoney(p.priceFrom) : 'Sob orçamento')
        ),
        h('span', { className: 'vt-pcard-cta', 'aria-hidden': 'true' }, 'Ver perfil')
      )
    );
  }

  function ReviewCard(p) {
    var tags = p.tags || [];
    return h('article', { className: cx('vt-review', p.className) },
      h('div', { className: 'vt-review-head' },
        h(Avatar, { initials: p.initials, name: p.author, tone: p.tone || 'info', size: 38, kind: 'client' }),
        h('div', { className: 'vt-review-who' },
          h('span', { className: 'vt-review-name' }, p.author),
          h('span', { className: 'vt-review-meta' }, [p.date, p.service].filter(Boolean).join(' · '))
        ),
        h(Rating, { value: p.rating, variant: 'stars', size: 13 })
      ),
      p.text ? h('p', { className: 'vt-review-text' }, p.text) : null,
      tags.length ? h('div', { className: 'vt-review-tags' }, tags.map(function (t, i) { return h(Badge, { key: i, tone: 'brand' }, t); })) : null,
      p.reply ? h('div', { className: 'vt-review-reply' },
        h('span', { className: 'vt-review-reply-k' }, 'Resposta de ' + p.reply.author),
        h('span', { className: 'vt-review-reply-v' }, p.reply.text)
      ) : null
    );
  }

  function StatusTimeline(p) {
    var steps = p.steps || [];
    var words = { done: 'concluída', current: 'etapa atual', pending: 'pendente' };
    return h('ol', { className: cx('vt-timeline', p.className), 'aria-label': p.label || 'Andamento do pedido' },
      steps.map(function (s, i) {
        var state = s.state || 'pending';
        return h('li', { key: i, className: cx('vt-step', 'vt-step-' + state), 'aria-current': state === 'current' ? 'step' : undefined },
          h('div', { className: 'vt-step-rail', 'aria-hidden': 'true' },
            h('span', { className: 'vt-step-dot' }, state === 'done' ? h(Icon, { name: 'check', size: 14, strokeWidth: 3 }) : null),
            i < steps.length - 1 ? h('span', { className: 'vt-step-line' }) : null
          ),
          h('div', { className: 'vt-step-body' },
            h('span', { className: 'vt-step-title' }, s.title, h('span', { className: 'vt-sr' }, ', ' + words[state])),
            s.sub ? h('span', { className: 'vt-step-sub' }, s.sub) : null
          )
        );
      })
    );
  }

  function QuoteCard(p) {
    var rows = p.rows || [];
    return h('section', { className: cx('vt-quote', p.className), 'aria-label': 'Orçamento de ' + p.from },
      h('div', { className: 'vt-quote-head' },
        h('span', { className: 'vt-quote-title' }, 'Orçamento de ' + p.from),
        p.validity ? h(Badge, { tone: 'warning' }, p.validity) : null
      ),
      h('p', { className: 'vt-quote-price' }, fmtMoney(p.price)),
      rows.length ? h('div', { className: 'vt-quote-rows' }, rows.map(function (r, i) {
        return h('span', { key: i, className: 'vt-quote-row' }, h(Icon, { name: r.icon, size: 17, strokeWidth: 2.2 }), r.text);
      })) : null,
      p.message ? h('p', { className: 'vt-quote-msg' }, p.message) : null,
      h('div', { className: 'vt-quote-actions' },
        h(Button, { variant: 'secondary', onClick: p.onDecline }, p.declineLabel || 'Recusar'),
        h(Button, { variant: 'primary', onClick: p.onAccept }, p.acceptLabel || 'Aceitar orçamento')
      )
    );
  }

  function PaymentReceipt(p) {
    var method = PAYMENT_METHODS.filter(function (m) { return m.value === p.method; })[0] || PAYMENT_METHODS[PAYMENT_METHODS.length - 1];
    return h('section', { className: cx('vt-receipt', p.className), 'aria-label': 'Pagamento confirmado' },
      h('div', { className: 'vt-receipt-head' },
        h('span', { className: 'vt-receipt-check' }, h(Icon, { name: 'check', size: 16, strokeWidth: 3 })),
        h('span', null, 'Pagamento confirmado')
      ),
      h('p', { className: 'vt-receipt-amount' }, fmtMoney(p.amount)),
      h('div', { className: 'vt-receipt-grid' },
        h('div', { className: 'vt-receipt-cell' },
          h('span', { className: 'vt-receipt-k' }, 'Forma'),
          h('span', { className: 'vt-receipt-v' }, h(Icon, { name: method.icon, size: 16, strokeWidth: 2.2 }), method.label)
        ),
        h('div', { className: 'vt-receipt-cell' },
          h('span', { className: 'vt-receipt-k' }, 'Data'),
          h('span', { className: 'vt-receipt-v' }, p.date)
        )
      ),
      h('p', { className: 'vt-receipt-foot' }, 'Confirmado por ' + p.confirmedBy + '. O pagamento foi combinado direto entre vocês.' + (p.note ? ' Observação: ' + p.note : '')),
      p.onDispute ? h('button', { type: 'button', className: 'vt-receipt-dispute', onClick: p.onDispute }, 'Não reconheço este pagamento') : null
    );
  }

  function BottomNav(p) {
    var items = p.items || [];
    return h('nav', { className: cx('vt-nav', p.className), 'aria-label': p.label || 'Navegação principal' },
      items.map(function (it, i) {
        return h('a', { key: i, className: 'vt-nav-item', href: it.href || '#', 'aria-current': it.current ? 'page' : undefined },
          h('span', { className: 'vt-nav-pill' }, h(Icon, { name: it.icon, size: 20, strokeWidth: it.current ? 2.2 : 2 })),
          it.label
        );
      })
    );
  }

  function TopBar(p) {
    return h('header', { className: cx('vt-topbar', p.className) },
      p.backHref || p.onBack ? h(IconButton, { icon: p.backIcon || 'chevron-left', label: p.backLabel || 'Voltar', href: p.backHref, onClick: p.onBack }) : h('span', { className: 'vt-topbar-spacer' }),
      h('h1', { className: 'vt-topbar-title' }, p.title),
      p.trailing ? p.trailing : h('span', { className: 'vt-topbar-spacer' })
    );
  }

  function ActionBar(p) {
    return h('div', { className: cx('vt-actionbar', p.className) },
      p.note ? h('p', { className: 'vt-actionbar-note' }, p.note) : null,
      h('div', { className: 'vt-actionbar-row' }, p.children)
    );
  }

  var api = {
    Icon: Icon,
    Button: Button,
    IconButton: IconButton,
    SearchField: SearchField,
    TextField: TextField,
    TextArea: TextArea,
    Chip: Chip,
    SegmentedControl: SegmentedControl,
    Switch: Switch,
    RatingInput: RatingInput,
    PaymentMethodPicker: PaymentMethodPicker,
    Badge: Badge,
    Avatar: Avatar,
    Rating: Rating,
    CategoryTile: CategoryTile,
    HeroCard: HeroCard,
    ProviderCard: ProviderCard,
    ReviewCard: ReviewCard,
    StatusTimeline: StatusTimeline,
    QuoteCard: QuoteCard,
    PaymentReceipt: PaymentReceipt,
    BottomNav: BottomNav,
    TopBar: TopBar,
    ActionBar: ActionBar,
    ICON_NAMES: Object.keys(ICONS),
    PAYMENT_METHODS: PAYMENT_METHODS
  };
  window.Vitrio = Object.assign(window.Vitrio || {}, api);
})();
