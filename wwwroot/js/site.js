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
// --- CHAT SYSTEM CLIENT ---

// 1. BIẾN TOÀN CỤC (Phải nằm ngoài cùng để không bị reset)
var clientConvId = 0;
var chatConnection = null;


// 2. HÀM ĐÓNG/MỞ (Đưa ra ngoài để onclick gọi được)
function toggleChatBox() {
    const chatBox = $("#customer-chat-box");

    // NẾU CHƯA ĐĂNG NHẬP
    if (!window.chatConfig.isLoggedIn) {
        chatBox.html(`
            <div class="chat-header d-flex justify-content-between align-items-center">
                <strong class="text-white">Hỗ trợ ManaPet</strong>
                <button onclick="$('#customer-chat-box').hide()" class="btn-close btn-close-white"></button>
            </div>
            <div class="chat-guest-overlay" style="padding: 20px; text-align: center; background:white; height:300px; display: flex; flex-direction: column; justify-content: center;">
                <i class="fas fa-lock mb-3" style="font-size:30px; color:#ccc;"></i>
                <h4>Bạn cần đăng nhập!</h4>
                <p>Vui lòng đăng nhập để chat với nhân viên hỗ trợ.</p>
                <a href="/User/Login" class="btn btn-primary btn-sm">Đến trang Đăng nhập</a>
            </div>
        `);
        chatBox.fadeToggle(300);
        return;
    }

    // NẾU ĐÃ ĐĂNG NHẬP
    chatBox.fadeToggle(300, function () {
        if ($(this).is(":visible")) {
            initChat();
        }
    });
}

// 3. KHỞI TẠO CHAT
async function initChat() {
    const uId = window.chatConfig.userId;

    if (uId === "0" || uId === "") {
        console.error("Chưa có UserId hợp lệ");
        return;
    }

    // Nếu đã có ID rồi thì chỉ cần đảm bảo kết nối SignalR còn sống
    if (clientConvId !== 0) {
        if (!chatConnection) setupSignalR();
        return;
    }

    
    try {
        const res = await fetch(`/api/chat/GetHistory/${uId}`);
        const result = await res.json();

        if (result.success) {
            clientConvId = result.conversationId;
            const chatBody = document.getElementById("customer-messages");
            chatBody.innerHTML = ""; // Xóa tin nhắn "Chào bạn" mặc định nếu muốn

            result.data.forEach(msg => appendMsgUI(msg.sender, msg.text, msg.time));
            setupSignalR();
        }
    } catch (err) {
        console.error("Lỗi load lịch sử:", err);
    }
}

// 4. KẾT NỐI SIGNALR
function setupSignalR() {
    if (chatConnection) return;

    chatConnection = new signalR.HubConnectionBuilder()
        .withUrl("/chatHub")
        .withAutomaticReconnect()
        .build();

    chatConnection.on("ReceiveMessage", function (role, message, time, convId, targetUserId) {
        // Chỉ nhận tin từ Admin và đúng cuộc hội thoại này
        if (convId === clientConvId && role === "Admin") {
            appendMsgUI(role, message, time);
        }
    });

    chatConnection.start().catch(err => console.error("SignalR Start Error:", err));
}

// 5. HIỂN THỊ TIN NHẮN
function appendMsgUI(sender, text, time) {
    const chatBody = document.getElementById("customer-messages");
    if (!chatBody) return;

    const isMe = (sender !== "Admin");

    const msgHtml = `
        <div style="display: flex; flex-direction: column; margin-bottom: 10px; align-items: ${isMe ? 'flex-end' : 'flex-start'}">
            <div style="max-width: 75%; padding: 8px 12px; border-radius: 15px; 
                        background: ${isMe ? '#0084ff' : '#e4e6eb'}; 
                        color: ${isMe ? '#fff' : '#000'}; word-wrap: break-word;">
                ${text}
            </div>
            <span style="font-size: 10px; color: gray; margin-top: 2px;">${time}</span>
        </div>`;

    chatBody.insertAdjacentHTML('beforeend', msgHtml);
    chatBody.scrollTop = chatBody.scrollHeight;
}

// 6. SỰ KIỆN GỬI TIN (Dùng $(document).ready để gán sự kiện 1 lần)
$(document).ready(function () {
    $(document).on("click", "#btn-customer-send", function () {
        let input = $("#customer-input");
        let msg = input.val();

        if (msg.trim() === "" || clientConvId === 0) return;

        let now = new Date();
        let timeStr = now.getHours().toString().padStart(2, '0') + ":" + now.getMinutes().toString().padStart(2, '0');

        // Hiện tin nhắn ngay (Optimistic UI)
        appendMsgUI("Customer", msg, timeStr);
        input.val("").focus();

        // Gửi lên Hub
        if (chatConnection && chatConnection.state === signalR.HubConnectionState.Connected) {
            chatConnection.invoke("SendMessage", clientConvId, parseInt(window.chatConfig.userId), msg, "Customer")
                .catch(err => console.error("Lỗi SendMessage:", err));
        } else {
            console.error("SignalR chưa kết nối!");
            // Thử kết nối lại nếu mất
            setupSignalR();
        }
    });

    $(document).on("keypress", "#customer-input", function (e) {
        if (e.which == 13) {
            $("#btn-customer-send").click();
            e.preventDefault();
        }
    });
});
//Chat box admin



