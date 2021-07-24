import React, { Component } from 'react';
import { setChonkyDefaults } from 'chonky';
import { ChonkyIconFA } from 'chonky-icon-fontawesome';
import { VFSReadOnly } from './chonky/VFSReadOnly';

setChonkyDefaults({ iconComponent: ChonkyIconFA });

export class Home extends Component {
  static displayName = Home.name;

  render () {
    return (
      <div>
        <h1>Wellcome to AutoCheck!</h1>
        <p>Step 1 - Select the AutoCheck's YAML script that you want to launch:</p>
        <VFSReadOnly instanceId="1" />
      </div>
    );
  }
}