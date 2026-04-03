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


//Chat box
// Hàm này để ngoài cùng để nút onclick="toggleChatBox()" luôn gọi được
function toggleChatBox() {
    const chatBox = $("#customer-chat-box");
    chatBox.fadeToggle(300, function () {
        // Nếu khung chat hiện lên thì mới đi bốc dữ liệu cũ
        if (chatBox.is(":visible")) {
            console.log("Đã mở chat, đang gọi openChat...");
            openChat();
        }
    });
}

async function openChat() {
    const uId = window.chatConfig.userId;
    const sId = localStorage.getItem("ManaPet_Session");

    console.log("Đang lấy lịch sử cho:", { uId, sId }); // Log cái này xem uId có còn rỗng không

    const res = await fetch(`/User/GetChatHistory?sessionId=${sId}&userId=${uId}`);
    const result = await res.json();

    console.log("Kết quả từ Server:", result); // Nếu success: false hoặc data rỗng thì lỗi ở Controller

    if (result.success && result.data) {
        const chatBody = document.getElementById("customer-messages");
        chatBody.innerHTML = ""; // Xóa trắng để load mới

        result.data.forEach(msg => {
            // Đảm bảo hàm này của ông đang chạy đúng
            appendMessageToUI(msg.sender, msg.text, msg.time);
        });
    }
}

function appendMessageToUI(senderType, message, time) {
    const chatBody = document.getElementById("customer-messages");
    if (!chatBody) return;

    const isAdmin = (senderType === "Admin");

    const msgHtml = `
        <div class="message-wrapper ${isAdmin ? 'from-admin' : 'from-me'}">
            <div class="message-content shadow-sm">
                ${message}
            </div>
            <span class="message-time">${time}</span>
        </div>`;

    chatBody.insertAdjacentHTML('beforeend', msgHtml);
    chatBody.scrollTop = chatBody.scrollHeight;
}

$(document).ready(function () {
    // 1. Khởi tạo Session cho khách
    let sessionId = localStorage.getItem("ManaPet_Session");
    if (!sessionId) {
        sessionId = "SESS_" + Math.random().toString(36).substr(2, 9);
        localStorage.setItem("ManaPet_Session", sessionId);
    }

    // 2. Kết nối SignalR
    var connection = new signalR.HubConnectionBuilder()
        .withUrl("/chatHub")
        .build();

    connection.on("ReceiveMessage", function (role, message, time, sId) {
        let mySessionId = localStorage.getItem("ManaPet_Session");
        let myUserId = window.chatConfig.userId;

        if (role === "Admin" || sId === mySessionId || (myUserId !== "" && sId === myUserId)) {
            // Dùng chung một hàm duy nhất để giao diện luôn ĐẸP như nhau
            appendMessageToUI(role, message, time);
        }
    });

    connection.start().then(() => console.log("Chat Connected!")).catch(err => console.error(err));

    // 3. Xử lý gửi tin nhắn
    $("#btn-customer-send").click(function () {
        let msg = $("#customer-input").val();
        if (msg.trim() === "") return;

        let sId = localStorage.getItem("ManaPet_Session");
        let uId = window.chatConfig.userId;
        let uRole = window.chatConfig.userRole;
        //console.log("Gửi tin nhắn:", { sId, msg, uId, uRole });

        connection.invoke("SendMessage", sId, msg, uId, uRole)
            .then(() => {
                $("#customer-input").val("").focus();
            })
            .catch(err => console.error("Gửi tin thất bại:", err));
    });

    // Enter để gửi
    $("#customer-input").keypress(function (e) {
        if (e.which == 13) {
            $("#btn-customer-send").click();
            e.preventDefault();
        }
    });
});
//Chat box