var Payment = function () {
    var init = function () {
        var form = document.querySelector('#PaymentForm');
        var nonceInput = document.querySelector('#PaymentMethodNonce');
        var clientToken = $('#ClientToken').val();

        braintree.dropin.create({
            authorization: clientToken,
            container: '#Dropin'
        }, function (error, instance) {
                if (error && error !== null) {
                    console.log(error);
                    return;
                }

                form.style.display = 'block';

                form.addEventListener('submit', function (event) {
                    event.preventDefault();

                    instance.requestPaymentMethod(function (error, payload) {
                        if (error && error !== null) {
                            console.log(error);
                            return;
                        }

                        nonceInput.value = payload.nonce;
                        form.submit();
                    });
                });
        });
    };

    return {
        init: init
    };
}();
