#!/bin/bash

set -ex

source ./00.build@travis-ci/activate.sh


dotnet build CommonUtilities.NET.Core.Standard.sln

# dotnet test