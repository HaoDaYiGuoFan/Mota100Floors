/* 魔塔 100 层 GDD 网站小脚本：导航高亮 + 表内过滤 */
(function () {
  'use strict';

  // 1) 根据当前页面标记导航高亮
  var current = document.body.getAttribute('data-page') || '';
  var links = document.querySelectorAll('.site-nav a');
  links.forEach(function (a) {
    if (a.getAttribute('data-page') === current) a.classList.add('active');
  });

  // 2) 所有带 data-filter 的输入框：按列内容过滤表格
  var inputs = document.querySelectorAll('input[data-filter]');
  inputs.forEach(function (input) {
    input.addEventListener('input', function () {
      var tableId = input.getAttribute('data-filter');
      var table = document.getElementById(tableId);
      if (!table) return;
      var kw = input.value.trim().toLowerCase();
      var rows = table.querySelectorAll('tbody tr');
      rows.forEach(function (tr) {
        var text = tr.textContent.toLowerCase();
        tr.style.display = (!kw || text.indexOf(kw) !== -1) ? '' : 'none';
      });
    });
  });

  // 3) 给引用锚点加平滑滚动（已由 CSS scroll-behavior 处理，此处兜底）
  document.querySelectorAll('a[href^="#"]').forEach(function (a) {
    a.addEventListener('click', function (e) {
      var id = a.getAttribute('href').slice(1);
      var el = id && document.getElementById(id);
      if (el) {
        e.preventDefault();
        el.scrollIntoView({ behavior: 'smooth', block: 'start' });
      }
    });
  });
})();