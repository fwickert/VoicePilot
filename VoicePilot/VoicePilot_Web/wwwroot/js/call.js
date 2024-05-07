function OnlySpinOn($spin) {
    $spin.removeClass("invisible");
    $spin.css("display", "inline-block");
}

function OnlySpinOff($spin) {
    $spin.addClass("invisible");
    $spin.css("display", "none");
}

function GetView($spin, $result, $url, $data) {    
    OnlySpinOn($spin);
    if ($url === undefined) {
        $url = $result.data("url")
    }

    $(".btn").addClass("disabled");

    if ($url === undefined) {
        $url = $result.data("url")
    }

    $.ajax({
        type: "Post",
        data: {
            input: $data
        },
        url: $url,
        cache: false
    })
        .done(function (result) {
            $result.html(result);
            //result is json format create object with
            var action = JSON.parse(result);
            moveBot(action.action, action.detail);
            OnlySpinOff($spin);
            $(".btn").removeClass("disabled");
        }).fail(function (jqXHR, textStatus, errorThrown) {
            $result.html(textStatus);
            OnlySpinOff($spin);
            $(".btn").removeClass("disabled");
        });

}