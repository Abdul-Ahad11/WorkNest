$(document).ready(function () {
    $('#bidForm').on('submit', function (e) {
        e.preventDefault();

        var amount = parseFloat($('#bidAmount').val());
        var proposal = $('#bidProposal').val().trim();
        var days = parseInt($('#bidDeliveryDays').val());

        if (!amount || amount <= 0) {
            showBidError('Please enter a valid bid amount.');
            return;
        }
        if (proposal.length < 20) {
            showBidError('Proposal must be at least 20 characters.');
            return;
        }
        if (!days || days < 1) {
            showBidError('Please enter valid delivery days.');
            return;
        }

        hideBidError();

        var btn = $('#bidSubmitBtn');
        btn.prop('disabled', true).html(
            '<span class="spinner-border spinner-border-sm me-1"></span>Submitting...'
        );

        var token = $('input[name="__RequestVerificationToken"]').val();

        var payload = {
            projectId: parseInt($('#projectId').val()),
            amount: amount,
            proposalMessage: proposal,
            deliveryDays: days
        };

        $.ajax({
            url: '/Bid/SubmitAjax',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(payload),
            headers: { 'RequestVerificationToken': token },
            success: function (response) {
                if (response.success) {
                    $('#bidForm').slideUp(300, function () {
                        $('#bidSuccessMsg')
                            .html('<div class="alert alert-success">' +
                                  response.message +
                                  '</div>')
                            .slideDown(300);
                    });
                } else {
                    showBidError(response.message || 'Submission failed.');
                    btn.prop('disabled', false).html('Submit Bid');
                }
            },
            error: function (xhr) {
                var msg = 'An error occurred. Please try again.';
                if (xhr.status === 401) msg = 'Please login to submit a bid.';
                if (xhr.status === 403) msg = 'Only freelancers can submit bids.';
                showBidError(msg);
                btn.prop('disabled', false).html('Submit Bid');
            }
        });
    });

    $(document).on('click', '.award-btn', function () {
        var bidId = $(this).data('bid-id');
        var freelancerName = $(this).data('freelancer');
        if (!confirm('Award this project to ' + freelancerName + '?')) return;

        var btn = $(this);
        btn.prop('disabled', true).html('Awarding...');
        var token = $('input[name="__RequestVerificationToken"]').val();

        $.ajax({
            url: '/Project/AwardAjax',
            type: 'POST',
            data: { bidId: bidId, __RequestVerificationToken: token },
            success: function (response) {
                if (response.success) {
                    location.reload();
                } else {
                    alert(response.message || 'Could not award project.');
                    btn.prop('disabled', false).html('Award');
                }
            },
            error: function () {
                alert('Error occurred. Please try again.');
                btn.prop('disabled', false).html('Award');
            }
        });
    });

    function showBidError(msg) {
        $('#bidErrorMsg').html(
            '<div class="alert alert-danger">' + msg + '</div>'
        ).show();
    }

    function hideBidError() {
        $('#bidErrorMsg').hide().html('');
    }
});