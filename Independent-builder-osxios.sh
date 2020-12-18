# dropout function definition
function try {
    "$@"
    local status=$?
    if [ $status -ne 0 ]; then
        echo "error with $1" >&2
		exit $status
    fi
    return $status
}

LIB_NAME=CurlThinLib

try cd CurlThinLib

# does not return error code correctly.
#$BIN_PATH/xbuild /p:Configuration=Release Independent-OSXiOS.sln
nuget restore CurlThinLib.sln
msbuild /p:Configuration=Release CurlThinLib.sln

try cd ..

try rm bin/OSX/Release/Unity* bin/iOS/Release/Unity* bin/Release/Unity*

try echo "{ \"version\":\"${DATEBLD}\", \"commit\":\"${COMMITSHA}\" }" >> Version_CurlThinLib.txt

# this one fails if the build was not correct
try tar -czf $LIB_NAME.tar.gz bin Version_CurlThinLib.txt
exit 0
