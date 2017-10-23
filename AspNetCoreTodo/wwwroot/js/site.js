$(document).ready(function() {
    // Wire up the add button to send the new item to the server
    $('#add-item-button').on('click', addItem);
    // Wire up all of the checkboxes to run markCompleted()
    $('.done-checkbox').on('click', function (e) {
        markCompleted(e.target);
    });
});

function getToken() {
    return token = $('input[name="__RequestVerificationToken"]').val();
}

function addItem() {
    $('#add-item-error').hide();
    var newTitle = $('#add-item-title').val();

    $.post('/Todo/AddItem', { __RequestVerificationToken: getToken(), title: newTitle }, function () {
        window.location = '/Todo';
    })
        .fail(function (data) {
            console.log(data);
            if (data && data.responseJSON) {
                var firstError = data.responseJSON[Object.keys(data.responseJSON)[0]];
                $('#add-item-error').text(firstError);
                $('#add-item-error').show();
            }
        });
}

function markCompleted(checkbox) {
    checkbox.disabled = true;

    $.post('/Todo/MarkDone', { __RequestVerificationToken: getToken(), id: checkbox.name }, function () {
        var row = checkbox.parentElement.parentElement;
        $(row).addClass('done');
    });
}