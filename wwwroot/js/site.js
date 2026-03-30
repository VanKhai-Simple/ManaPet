// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
$(document).ready(function () {
    // Khởi tạo Tooltips
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'))
    var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl)
    });

    // Xử lý Active Link
    var currentPath = window.location.pathname;
    $('.navbar-nav .nav-link').each(function () {
        var href = $(this).attr('href');
        if (currentPath === href) {
            $(this).addClass('active');
        }
    });
});


// Back to Top Button
const backToTopBtn = document.getElementById("backToTop");

// Hiện nút khi cuộn xuống 300px
window.onscroll = function () {
    if (document.body.scrollTop > 300 || document.documentElement.scrollTop > 300) {
        backToTopBtn.style.display = "block";
    } else {
        backToTopBtn.style.display = "none";
    }
};

// Click để lên đầu trang mượt mà
backToTopBtn.onclick = function () {
    window.scrollTo({ top: 0, behavior: 'smooth' });
};

if (window.location.hash && window.location.hash == '#_=_') {
    if (window.history.replaceState) {
        window.history.replaceState("", document.title, window.location.pathname + window.location.search);
    } else {
        // Dành cho trình duyệt cũ
        window.location.hash = "";
    }
}