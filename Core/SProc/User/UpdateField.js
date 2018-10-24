function UpdateField(id, fields, values) {
    var context = getContext();
    var collection = context.getCollection();
    var response = context.getResponse();

    // log(toJson({
    //     id: id,
    //     fields: fields,
    //     values: values,
    // }));

    var query = {
        query: "select * from root r where r.id = @id",
        parameters: [{
            name: "@id",
            value: id
        }]
    };

    //queryDocuments
    var accept = collection.queryDocuments(collection.getSelfLink(), query,
        resultCallback);
    if (!accept) throw "Not allow to query";

    function resultCallback(err, documents, responseOptions) {
        if (err) throw new Error("Error" + err.message);
        if (documents.length != 1) throw 'Unable to find any document';

        var doc = documents[0];

        //update doc
        for (i = 0; i < fields.length; i++) {
            doc[fields[i]] = values[i];
        }

        //replaceDocument
        var accept = collection.replaceDocument(doc._self, doc, {
                etag: doc.etag
            },
            function (err, docReplaced) {
                if (err) throw "Unable to update document, abort";

                //set response result
                response.setBody(docReplaced);
            });
        if (!accept) throw "Not allow to update document, abort";

        return;
    }
}

function log(text) {
    console.log(text + "\n");
}

function toJson(object) {
    return JSON.stringify(object, null, 4);
}