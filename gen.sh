#!/bin/bash

# Check if filename parameter is provided
if [ $# -eq 0 ]; then
    echo "Usage: $0 <filename-without-extension>"
    echo "Example: $0 gryphon-plan"
    exit 1
fi

filename="$1"
input_file="${filename}.md"
output_file="docx/${filename}.docx"

# Check if input markdown file exists
if [ ! -f "$input_file" ]; then
    echo "Error: $input_file not found"
    exit 1
fi

# Generate docx file
pandoc "$input_file" -o "$output_file" --reference-doc=template.docx

echo "Generated: $output_file"