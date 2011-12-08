exports.bufferToObjects = function (buf) {
    var dataStrList = buf.toString('utf8');
    var dataList = dataStrList.split("\\n");
    var retval = [];
    for (var i = 0; i < dataList.length; i++) {
        dataStr = dataList[i];
        if (dataStr.length > 0) {
            try {
                retval.push(JSON.parse(dataStr));
            } catch (Exception) {
            }
        }
    }
    return retval;
};