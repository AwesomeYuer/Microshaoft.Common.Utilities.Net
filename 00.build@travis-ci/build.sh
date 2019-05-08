#!/bin/bash
#pwd
set -ex
#pwd
source ./00.build@travis-ci/activate.sh

#pwd
dotnet build

# dotnet test