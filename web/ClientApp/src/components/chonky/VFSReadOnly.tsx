/**
 * @author Timur Kuzhagaliyev <tim.kuzh@gmail.com>
 * @copyright 2020
 * @license MIT
 */

 import {
    ChonkyActions,
    ChonkyFileActionData,
    FileArray,
    FileBrowser,
    FileContextMenu,
    FileData,
    FileHelper,
    FileList,
    FileNavbar,
    FileToolbar,
    setChonkyDefaults,
} from 'chonky';
import { ChonkyIconFA } from 'chonky-icon-fontawesome';
import React, { useCallback, useMemo, useState } from 'react';

//import { showActionNotification, useStoryLinks } from './util';
import ProdFsMap from './files.production.json';

setChonkyDefaults({ iconComponent: ChonkyIconFA });

const rootFolderId = ProdFsMap.rootFolderId;
const fileMap = (ProdFsMap.fileMap as unknown) as {
    [fileId: string]: FileData & { childrenIds: string[] };
};

export const useFiles = (currentFolderId: string): FileArray => {
    return useMemo(() => {
        const currentFolder = fileMap[currentFolderId];
        const files = currentFolder.childrenIds
            ? currentFolder.childrenIds.map((fileId: string) => fileMap[fileId] ?? null)
            : [];
        return files;
    }, [currentFolderId]);
};

export const useFolderChain = (currentFolderId: string): FileArray => {
    return useMemo(() => {
        const currentFolder = fileMap[currentFolderId];

        const folderChain = [currentFolder];

        let parentId = currentFolder.parentId;
        while (parentId) {
            const parentFile = fileMap[parentId];
            if (parentFile) {
                folderChain.unshift(parentFile);
                parentId = parentFile.parentId;
            } else {
                parentId = null;
            }
        }

        return folderChain;
    }, [currentFolderId]);
};

export const useFileActionHandler = (
    setCurrentFolderId: (folderId: string) => void
) => {
    return useCallback(
        (data: ChonkyFileActionData) => {
            var filePath = "";

            if (data.id === ChonkyActions.OpenFiles.id) {
                const { targetFile, files } = data.payload;
                const fileToOpen = targetFile ?? files[0];
                if (fileToOpen && FileHelper.isDirectory(fileToOpen)) {
                    setCurrentFolderId(fileToOpen.id);
                    return;
                }
                
                filePath = getFilePath(fileToOpen);
                
                var div = document.querySelector('#step-1');
                if(div != null) div.style.display = 'none';

                div = document.querySelector('#step-2');
                if(div != null) div.style.visibility = 'block';
            
            }
            
            //TODO:
            //  -1. remove the call to showActionNotification
            //  - 2. remove the dependencies over 'util' and 'override.css'
            //  3. hide the file browser (use react approach using states)
            //  4. display an infinite loading bar 
            //  5. display a log under the loading bar
            //  6. call to AutoCheck's core on parallel 
            //  7. get the entire execution when done and display the log
            
            //showActionNotification(data);


        },
        [setCurrentFolderId]
    );
};
function getFilePath(current: FileData){
    var path = [];
    var filePath = "";

    while(current.parentId != null){
        path.push(current.name);
        current = fileMap[current.parentId];
    }

    path.push(current.name);    
    while(path.length > 0){
        filePath += path.pop() + "/";
    }

    return filePath.slice(0, -1);  
}

export const VFSReadOnly: React.FC<{ instanceId: string }> = (props) => {
    const [currentFolderId, setCurrentFolderId] = useState(rootFolderId);
    const files = useFiles(currentFolderId);
    const folderChain = useFolderChain(currentFolderId);
    const handleFileAction = useFileActionHandler(setCurrentFolderId);
    return (
        <div style={{ height: 400 }}>
            <FileBrowser
                instanceId={props.instanceId}
                files={files}
                folderChain={folderChain}
                onFileAction={handleFileAction}
                thumbnailGenerator={(file: FileData) =>
                    file.thumbnailUrl ? `https://chonky.io${file.thumbnailUrl}` : null
                }
            >
                <FileNavbar />
                <FileToolbar />
                <FileList />
                <FileContextMenu />
            </FileBrowser>
        </div>
    );
};