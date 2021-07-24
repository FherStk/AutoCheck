import React, { Component } from 'react';
import { FullFileBrowser } from 'chonky';
import { setChonkyDefaults } from 'chonky';
import { ChonkyIconFA } from 'chonky-icon-fontawesome';

export const MyFileBrowser = () => {
  const files = [
      { id: 'lht', name: 'Projects', isDir: true },
      {
          id: 'mcd',
          name: 'chonky-sphere-v2.png',
          thumbnailUrl: 'https://chonky.io/chonky-sphere-v2.png',
      },
  ];
  const folderChain = [{ id: 'xcv', name: 'Demo', isDir: true }];
  return (
      <div style={{ height: 300 }}>
          <FullFileBrowser files={files} folderChain={folderChain} />
      </div>
  );
};

export class Home extends Component {
  static displayName = Home.name;

  render () {
    return (
      <div>
        <h1>Wellcome to AutoCheck!</h1>
        <p>Step 1 - Select the AutoCheck's YAML script that you want to launch:</p>
        <MyFileBrowser></MyFileBrowser>
      </div>
    );
  }
}

setChonkyDefaults({ iconComponent: ChonkyIconFA });