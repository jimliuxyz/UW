
function sp(docToCreate, antiqueYear) {
    var collection = getContext().getCollection();
    var response = getContext().getResponse();

    if (docToCreate.Year != undefined && docToCreate.Year < antiqueYear) {
        docToCreate.antique = true;
    }

    // collection.createDocument(collection.getSelfLink(), docToCreate, {},
    //     function (err, docCreated, options) {
    //         if (err) throw new Error('Error while creating document: ' + err.message);
    //         if (options.maxCollectionSizeInMb == 0) throw 'max collection size not found';
    //         response.setBody(docCreated);
    //     });

    var id = 1;
    var query = 'SELECT * FROM x';
    // var query = 'SELECT * FROM x WHERE x.id = "123"';


    collection.queryDocuments(collection.getSelfLink(), query, { pageSize: 100 }, function (err, documents) {


        var x = __.filter(function (doc) {
            // console.log("=====\n")
            console.log(JSON.stringify(doc, null, 4))
            // console.log("=====\n")
                    // console.log("ooo");

            return true
        }, function (err, feed, options) {
        console.log("xxx");
            if (err) throw err;
        });
        console.log(JSON.stringify(x, null, 4))

        // console.log("=====\n")
        // console.log(JSON.stringify(documents, null, 4))
        // console.log("=====\n")

        // if (!documents || !documents.length) response.setBody({ error: 'No documents were found.' });
        // // else response.setBody(documents);
        // else response.setBody({ result: documents });
    });
    response.setBody({ error: 'done....' });

}