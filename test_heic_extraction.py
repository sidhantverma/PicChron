#!/usr/bin/env python3
"""
Test script to extract date from HEIC files using Python
"""
import sys
from pathlib import Path
from datetime import datetime
import json

def test_heic_with_pillow(file_path):
    """Try to extract EXIF using Pillow"""
    try:
        from PIL import Image
        from PIL.ExifTags import TAGS
        
        print(f"Testing with Pillow...")
        img = Image.open(file_path)
        
        if hasattr(img, '_getexif'):
            exif_data = img._getexif()
            if exif_data:
                # Tag 36867 is DateTimeOriginal
                # Tag 306 is DateTime
                for tag_id, value in exif_data.items():
                    tag_name = TAGS.get(tag_id, tag_id)
                    print(f"  {tag_name} ({tag_id}): {value}")
                    if tag_id == 36867:  # DateTimeOriginal
                        print(f"  → Found DateTimeOriginal: {value}")
                        return value
        return None
    except Exception as e:
        print(f"Pillow failed: {e}")
        return None

def test_heic_with_piexif(file_path):
    """Try to extract EXIF using piexif"""
    try:
        import piexif
        
        print(f"Testing with piexif...")
        exif_dict = piexif.load(file_path)
        
        for ifd_name in ("0th", "Exif", "GPS", "1st"):
            ifd = exif_dict[ifd_name]
            for tag in ifd:
                tag_name = piexif.TAGS[ifd_name][tag]["name"]
                value = ifd[tag]
                
                # Decode if bytes
                if isinstance(value, bytes):
                    try:
                        value = value.decode('utf-8')
                    except:
                        pass
                
                print(f"  {tag_name}: {value}")
                
                # Look for date tags
                if tag_name in ["DateTime", "DateTimeOriginal", "DateTimeDigitized"]:
                    print(f"  → Found {tag_name}: {value}")
                    return value
        return None
    except Exception as e:
        print(f"piexif failed: {e}")
        return None

def test_heic_with_exifread(file_path):
    """Try to extract EXIF using exifread"""
    try:
        import exifread
        
        print(f"Testing with exifread...")
        with open(file_path, 'rb') as f:
            tags = exifread.process_file(f, details=False)
            
            for tag in tags:
                print(f"  {tag}: {tags[tag]}")
                
                if "DateTime" in tag:
                    print(f"  → Found {tag}: {tags[tag]}")
                    return str(tags[tag])
        return None
    except Exception as e:
        print(f"exifread failed: {e}")
        return None

def test_heic_with_raw_metadata(file_path):
    """Try to read raw metadata"""
    try:
        from PIL import Image
        
        print(f"Testing with PIL raw metadata...")
        img = Image.open(file_path)
        
        # Check for metadata in info
        if hasattr(img, 'info'):
            print(f"  Image info keys: {img.info.keys()}")
            for key, value in img.info.items():
                print(f"  {key}: {value}")
        
        return None
    except Exception as e:
        print(f"Raw metadata failed: {e}")
        return None

if __name__ == "__main__":
    file_path = r"C:\Users\sidhantverma\Downloads\Import\IMG_8286.HEIC"
    
    if not Path(file_path).exists():
        print(f"ERROR: File not found: {file_path}")
        sys.exit(1)
    
    print(f"Testing HEIC file extraction: {file_path}")
    print(f"File exists: {Path(file_path).stat().st_size} bytes")
    print()
    
    # Try different methods
    date = test_heic_with_pillow(file_path)
    if date:
        print(f"\n✓ SUCCESS with Pillow: {date}")
        sys.exit(0)
    
    print()
    date = test_heic_with_piexif(file_path)
    if date:
        print(f"\n✓ SUCCESS with piexif: {date}")
        sys.exit(0)
    
    print()
    date = test_heic_with_exifread(file_path)
    if date:
        print(f"\n✓ SUCCESS with exifread: {date}")
        sys.exit(0)
    
    print()
    test_heic_with_raw_metadata(file_path)
    
    print("\nNo date found with any method")
    sys.exit(1)
