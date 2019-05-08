#!/bin/bash

set -ex

source activate.sh


dotnet build CommonUtilities.NET.Core.Standard.sln

# dotnet test