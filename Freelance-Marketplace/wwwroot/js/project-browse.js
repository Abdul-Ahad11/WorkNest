$(document).ready(function () {
    var searchTimeout;
    var currentPage = 1;

    $('#searchInput').on('keyup', function () {
        clearTimeout(searchTimeout);
        searchTimeout = setTimeout(function () {
            currentPage = 1;
            performSearch();
        }, 300);
    });

    $('#categoryFilter, #sortBy').on('change', function () {
        currentPage = 1;
        performSearch();
    });

    $('#minBudget, #maxBudget').on('change', function () {
        currentPage = 1;
        performSearch();
    });

    $('#searchBtn').on('click', function () {
        currentPage = 1;
        performSearch();
    });

    $('#clearBtn').on('click', function () {
        $('#searchInput').val('');
        $('#categoryFilter').val('');
        $('#minBudget').val('');
        $('#maxBudget').val('');
        $('#sortBy').val('newest');
        currentPage = 1;
        performSearch();
    });

    function performSearch() {
        var params = {
            keyword: $('#searchInput').val(),
            categoryId: $('#categoryFilter').val(),
            minBudget: $('#minBudget').val(),
            maxBudget: $('#maxBudget').val(),
            sortBy: $('#sortBy').val(),
            page: currentPage
        };

        showLoading();
        $.ajax({
            url: '/Project/SearchAjax',
            type: 'GET',
            data: params,
            success: function (data) {
                hideLoading();
                if (data.projects.length === 0) {
                    showEmptyState();
                } else {
                    renderProjects(data.projects);
                    renderPagination(data.totalPages, data.currentPage);
                    $('#resultsCount').text(
                        'Showing ' + data.projects.length + ' of ' + data.totalCount + ' projects'
                    );
                }
            },
            error: function () {
                hideLoading();
                showError('Failed to load projects. Please try again.');
            }
        });
    }

    function renderProjects(projects) {
        var html = '';
        projects.forEach(function (p) {
            var daysLeft = p.daysRemaining > 0
                ? p.daysRemaining + ' days left'
                : '<span class="text-danger">Expired</span>';
            var budget = p.maxBudget
                ? '$' + p.budget + ' - $' + p.maxBudget
                : '$' + p.budget;

            html += '<div class="col-lg-4 col-md-6 mb-4">' +
                '<div class="card h-100 shadow-sm project-card">' +
                '<div class="card-header d-flex justify-content-between">' +
                '<span class="badge bg-primary">' + escapeHtml(p.categoryName) + '</span>' +
                '<span class="badge bg-success">Open</span>' +
                '</div>' +
                '<div class="card-body">' +
                '<h5 class="card-title">' + escapeHtml(p.title) + '</h5>' +
                '<p class="card-text text-muted small">' +
                escapeHtml(p.description) + '</p>' +
                '<p class="fw-bold text-success mb-1">' + budget + '</p>' +
                '<p class="small text-muted">' + daysLeft + '</p>' +
                '</div>' +
                '<div class="card-footer d-flex justify-content-between">' +
                '<small class="text-muted">' + escapeHtml(p.clientName) +
                ' | ' + p.bidCount + ' bids</small>' +
                '<a href="/Project/Details/' + p.id +
                '" class="btn btn-sm btn-outline-primary">View</a>' +
                '</div></div></div>';
        });
        $('#projectsGrid').html(html);
        $('#emptyState').hide();
        $('#projectsGrid').show();
    }

    function renderPagination(totalPages, current) {
        if (totalPages <= 1) {
            $('#paginationControls').html('');
            return;
        }
        var html = '<nav><ul class="pagination justify-content-center">';
        for (var i = 1; i <= totalPages; i++) {
            var active = i === current ? 'active' : '';
            html += '<li class="page-item ' + active + '">' +
                '<a class="page-link page-btn" data-page="' + i +
                '" href="#">' + i + '</a></li>';
        }
        html += '</ul></nav>';
        $('#paginationControls').html(html);
    }

    $(document).on('click', '.page-btn', function (e) {
        e.preventDefault();
        currentPage = parseInt($(this).data('page'));
        performSearch();
        $('html, body').animate({ scrollTop: $('#projectsGrid').offset().top - 20 }, 300);
    });

    function showLoading() {
        $('#loadingSpinner').show();
        $('#projectsGrid').hide();
        $('#emptyState').hide();
    }

    function hideLoading() {
        $('#loadingSpinner').hide();
    }

    function showEmptyState() {
        $('#projectsGrid').hide();
        $('#emptyState').show();
        $('#paginationControls').html('');
        $('#resultsCount').text('No projects found');
    }

    function showError(msg) {
        $('#projectsGrid').html(
            '<div class="col-12"><div class="alert alert-danger">' + msg + '</div></div>'
        );
        $('#projectsGrid').show();
    }

    function escapeHtml(text) {
        if (!text) return '';
        return $('<div>').text(text).html();
    }
});