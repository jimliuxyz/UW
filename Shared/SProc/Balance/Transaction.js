/**
 * 
 * @param {*} action "pay" / "gain"
 * @param {*} txId 
 * @param {*} id 
 * @param {*} currency 
 * @param {*} amount 
 */
function UpdateField(action, txId, time, userId, currency, amount) {

    // check arguments
    if (!txId || !time || !userId || !currency || !amount)
        throw CodeExp(InvalidParameter);
    if (amount < 0)
        throw CodeExp(InvalidParameter);

    // log(toJson({
    //     arguments: arguments,
    // }));

    // define error code
    var InvalidParameter = { code: -1000, message: "Invalid Parameter" };
    var InvalidAction = { code: -1001, message: "Invalid Action" };
    var InsufficientBalance = { code: -1002, message: "Insufficient Balance" };

    // generate code exception
    function CodeExp(err, message_override) {

        if (message_override != undefined)
            err = { code: -1, message: message_override };

        return JSON.stringify({
            code: err.code,
            message: err.message
        });
    }



    // ~~ procedure start ~~
    var context = getContext();
    var collection = context.getCollection();
    var response = context.getResponse();

    var query = {
        query: "select * from root r where r.id = @id",
        parameters: [{
            name: "@id",
            value: userId
        }]
    };

    //queryDocuments
    var accept = collection.queryDocuments(collection.getSelfLink(), query,
        resultCallback);
    if (!accept) throw MsgExp("Not allow to query");

    function resultCallback(err, documents, responseOptions) {
        if (err) throw new Error("Error" + err.message);
        if (documents.length != 1) throw MsgExp('Unable to find any document of \'' + userId + '\'');

        var doc = documents[0];

        if (doc.balance[currency] === undefined)
            doc.balance[currency] = 0;

        if (action == "out") {
            if (amount > doc.balance[currency])
                throw CodeExp(InsufficientBalance);
            else if (doc.outBuf[txId] === undefined) {
                doc.outBuf[txId] = {
                    txId: txId,
                    currency: currency,
                    time: time,
                    receiptId: "",
                    amount: amount,
                    confirm: false,
                }
                doc.balance[currency] -= amount;
            }
        }
        else if (action == "in") {
            if (doc.outBuf[txId] === undefined) {
                doc.outBuf[txId] = {
                    txId: txId,
                    currency: currency,
                    time: time,
                    receiptId: "",
                    amount: amount,
                    confirm: true,
                }
                doc.balance[currency] += amount;
            }
        }
        else
            throw CodeExp(InvalidAction);



        //replaceDocument
        var accept = collection.replaceDocument(doc._self, doc, {
            etag: doc.etag
        },
            function (err, docReplaced) {
                if (err) throw MsgExp("Unable to update document, abort");

                //set response result
                response.setBody({
                    code: 0
                });
            });
        if (!accept) throw MsgExp("Not allow to update document, abort");

        return;
    }
}

function log(text) {
    console.log(text + "\n");
}

function toJson(object) {
    return JSON.stringify(object, null, 4);
}

function MsgExp(message) {
    return JSON.stringify({
        code: -1,   // undefined error
        message
    });
}
