#!/bin/bash

set -x

docker build . -t karayAkar/monacoEditorCSharp

docker images

docker push ka/monacoEditorCSharp
