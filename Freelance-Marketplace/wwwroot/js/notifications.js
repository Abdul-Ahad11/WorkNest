$(document).ready(function () {
    updateNotificationCount();
    setInterval(updateNotificationCount, 30000);

    function updateNotificationCount() {
        $.ajax({
            url: '/Notification/GetUnreadCount',
            type: 'GET',
            success: function (data) {
                var count = data.count;
                var badge = $('#notification-count');
                if (count > 0) {
                    badge.text(count).show();
                } else {
                    badge.hide();
                }
            },
            error: function () {
                console.error('Failed to fetch notification count');
            }
        });
    }

    $('#notification-bell').on('click', function (e) {
        e.preventDefault();
        $.ajax({
            url: '/Notification/GetLatestNotifications',
            type: 'GET',
            success: function (notifications) {
                renderNotificationDropdown(notifications);
            }
        });
    });

    function renderNotificationDropdown(notifications) {
        var html = '';
        if (notifications.length === 0) {
            html = '<li class="dropdown-item text-muted">No new notifications</li>';
        } else {
            notifications.forEach(function (n) {
                var readClass = n.isRead ? 'text-muted' : 'fw-bold';
                html += '<li><a class="dropdown-item ' + readClass +
                        ' notification-item" href="' + (n.link || '#') +
                        '" data-id="' + n.id + '">' +
                        '<small class="d-block">' + n.message + '</small>' +
                        '<small class="text-muted">' + n.timeAgo + '</small>' +
                        '</a></li>';
            });
            html += '<li><hr class="dropdown-divider"></li>';
            html += '<li><a class="dropdown-item text-center" ' +
                    'id="markAllRead" href="#">Mark all as read</a></li>';
        }
        $('#notification-list').html(html);
    }

    $(document).on('click', '.notification-item', function () {
        var notifId = $(this).data('id');
        var href = $(this).attr('href');
        $.post('/Notification/MarkAsRead', { id: notifId }, function () {
            updateNotificationCount();
        });
        if (href && href !== '#') {
            window.location.href = href;
            return false;
        }
    });

    $(document).on('click', '#markAllRead', function (e) {
        e.preventDefault();
        $.post('/Notification/MarkAllAsRead', function () {
            updateNotificationCount();
            $('#notification-list').html(
                '<li class="dropdown-item text-muted">No new notifications</li>'
            );
        });
    });
});